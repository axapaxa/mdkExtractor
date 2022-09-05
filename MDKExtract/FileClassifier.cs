using MDKExtract.ExtractorTypes;
using MDKExtract.FolderMetadata;
using MDKExtract.PaleteExtraction;
using MDKExtract.RawFileCompressors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MDKExtract
{
    public class FileClassifier
    {
        public static IEnumerable<RawFilePacker> RawPackers => new List<RawFilePacker>() { new TextureToPng(), new TextDetector(), new ScriptDetector() };

        public static Func<string, bool> IsPlausibleName => name =>
        {
            var regex = new Regex(@"^[a-zA-Z_0-9\-\$]+$");
            return regex.IsMatch(name);
        };
        public static Func<ExtractedModel, bool> IsPlausibleExtraction => model =>
        {
            var regex = new Regex(@"^[a-zA-Z_0-9\$]+$");
            return model.Data.All(x =>
            {
                var result = regex.IsMatch(x.Name);
                if (!result)
                {
                    Console.WriteLine(x.Name);
                }
                return result;
            });
        };

        public static async Task Decode(DirectoryInfo currentDirectory, Stream? stream, bool isRoot, FullFolderMeta meta, IEnumerable<IExtractor> extractors)
        {
            if (stream is null)
                return;
            if (stream.Length == 0)
                return;
            Console.WriteLine($"Extracting {meta.FolderName}");

            foreach(var extractor in extractors)
            {
                ExtractedModel model;
                try
                {
                    stream.Position = 0;
                    model = await extractor.Extract(stream);
                    if (!IsPlausibleExtraction(model))
                        throw new ArgumentException("Extraction is not plausibly correct");
                }
                catch (Exception e) { continue; }
                var subdir = currentDirectory.CreateSubdirectory(meta.FolderName + " (" + model.FileName + ")");
                var newParts = model.FileName.Length > 1 ? meta.PathParts.SkipLast(1).Append(model.FileName) : meta.PathParts;
                var metaPieces = model.Data.SelectMany(x => DetectMetaPiecesInFolderStructure.Decode(x.Data)).Concat(PalleteAppender.GetPalletes(model, extractor, new FullFolderMeta() { FolderName = newParts.Last(), MetaPieces = meta.MetaPieces, PathParts = newParts.ToArray() })).ToList();
                
                var tasks = model.Data.AsParallel().WithDegreeOfParallelism(Program.DecodingParalellism).Select(async data =>
                {
                    await Decode(subdir, data.Data, false, new FullFolderMeta() { FolderName = data.Name, MetaPieces = meta.MetaPieces.Concat(metaPieces).ToList(), PathParts = newParts.Append(data.Name).ToArray() }, extractors);
                    if ((data.PostData?.Length ?? 0) != 0) 
                        await Decode(subdir, data.PostData, false, new FullFolderMeta() { FolderName = data.Name+"(postdata)", MetaPieces = meta.MetaPieces.Concat(metaPieces).ToList(), PathParts = newParts.Append(data.Name).ToArray() }, extractors);
                }).ToList();
                await Task.WhenAll(tasks);
                return;
                
            }

            stream.Position = 0;
            var reader = new BinaryReader(stream);
            string ext;
            try
            {
                switch (reader.ReadInt32())
                {
                    case 1179011410: //RIFF
                        ext = ".wav";
                        break;
                    case 944130375: //GIF8
                        ext = ".gif";
                        break;
                    case 1065353216: //Appears to be animation data
                        ext = ".anim";
                        break;
                    default:
                        ext = ".dat";
                        break;
                }
            } catch
            {
                ext = ".dat";
            }
            

            if (ext == ".dat")
            {
                //Try some more before giving up
                var decoded = RawPackers.SelectMany(x => x.AttemptUnpack(stream, meta)).ToList();
                if (decoded.Any())
                {
                    var subDir = (decoded.Count == 1) ? currentDirectory : currentDirectory.CreateSubdirectory(meta.FolderName);
                    var index = 0;
                    foreach (var decodedSingle in decoded) //.Concat(new[] { (stream: stream, ext: ".dat") })
                    {
                        index++;
                        using var subFile = new FileStream(Path.Combine(subDir.FullName, meta.FolderName + index + decodedSingle.ext), FileMode.Create, FileAccess.Write);
                        decodedSingle.stream.Position = 0;
                        await decodedSingle.stream.CopyToAsync(subFile);
                    }
                    return;
                }
            }

            if (ext == ".dat")
                ext = EmptyDetector.IsEmpty(stream) ?? ext;
            if (ext == ".dat")
                ext = ModelDetector.TryExtensionFromStreamAndExtract(stream, Path.Combine(currentDirectory.FullName, meta.FolderName)) ?? ext;
            stream.Position = 0;

            /*if (isRoot && ext == ".dat")
                return;*/

            stream.Position = 0;
            using var file = new FileStream(Path.Combine(currentDirectory.FullName, meta.FolderName+ext), FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(file);
        }
    }
}
