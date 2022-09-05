using MDKExtract.ExtractorTypes;
using MDKExtract.FolderMetadata;
using MDKExtract.PaleteExtraction;
using MDKExtract.RawFileCompressors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MDKExtract
{
    class Program
    {
        public static int DecodingParalellism = 4;

        //public static DirectoryInfo GameDir = new DirectoryInfo(@"D:\GOG Games\mdk_original_installed\");
        //public static DirectoryInfo GameDir = new DirectoryInfo(@"D:\GOG Games\MDK\");
        public static DirectoryInfo GameDir = new DirectoryInfo(@"D:\jd2-downloads\MDK\MDK (1996-08-06) (beta demo)");
        static async Task DecodeGroup(string ext, IEnumerable<IExtractor> extractors, DirectoryInfo target, ICollection<IFolderMetadata> readMeta)
        {
            var filesToCheck = GameDir.GetFiles(ext, new EnumerationOptions() { RecurseSubdirectories = true });
            var parallelTasks = filesToCheck.AsParallel().WithDegreeOfParallelism(DecodingParalellism).Select(async file =>
            {
                using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                var fsLoaded = ExtractionUtils.GetStreamFromData(fs, (int)fs.Length);
                await FileClassifier.Decode(target, fsLoaded, true, new FullFolderMeta() { FolderName = file.Name, MetaPieces = new(readMeta), PathParts = new[] { file.Name} }, extractors);
            }).ToList();
            await Task.WhenAll(parallelTasks);
        }

        static async Task DecodeAll()
        {
            await GlobalPalletes.GetMainPallettes().ReadFallFiles(GameDir);
            await GlobalPalletes.GetMainPallettes().ReadMtoFiles(GameDir);
            await GlobalPalletes.GetMainPallettes().ReadStatsPalette(GameDir);
            await GlobalPalletes.GetMainPallettes().ReadStreamPalette(GameDir);
            var paletes = new DirectoryInfo("palete");
            var readMeta = paletes.GetFiles("*.*").Concat(GameDir.GetFiles("*.LBP", SearchOption.AllDirectories)).Select(x => x.OpenRead()).SelectMany(x => DetectMetaPiecesInFolderStructure.Decode(x)).Distinct().ToList();

            var target = new DirectoryInfo("extractions");
            //target.Delete(true);
            target.Create();

            var tasks = new List<Task>()
            {
                DecodeGroup("*.MTI", new IExtractor[] { new MtiExtractor(), new Mti1996Extractor() }, target, readMeta),
                DecodeGroup("*.SNI", new IExtractor[] { new DefaultSniExtractor(), new K_BFLIPDenseExtractor(), new Sni1996Extractor() }, target, readMeta),
                DecodeGroup("*.MTO", new IExtractor[] { new MtoExtractor(), new _3PartExtractor(), new MtiExtractor(), new Part2Extractor(), new Mto1996Extractor(), new Mto1996Part2Extractor() }, target, readMeta),
                DecodeGroup("*.BNI", new IExtractor[] { new StatsBniExtractor(), new K_BFLIPDenseExtractor() }, target, readMeta),
                DecodeGroup("*.CMI", new IExtractor[] {new CmiExtractor()}, target, readMeta),
                DecodeGroup("*.DTI", new IExtractor[] { new Dti5PartExtractor(), new DtiPart3Extractor() }, target, readMeta),
                DecodeGroup("*.LBB", new IExtractor[] { }, target, readMeta),
                DecodeGroup("*.FTI", new IExtractor[] { new FtiExtractor() }, target, readMeta),
                //Extra definitions for 1996 demo decoding ability
                DecodeGroup("*.LOV", new IExtractor[] { }, target, readMeta),
                DecodeGroup("*.BSP", new IExtractor[] { }, target, readMeta),
                DecodeGroup("*.ABB", new IExtractor[] { new K_BFLIPDenseExtractor() }, target, readMeta),
                DecodeGroup("*.LBA", new IExtractor[] { new K_BFLIPDenseExtractor()}, target, readMeta),
            };
            await Task.WhenAll(tasks);

            //Console.WriteLine(model.Data.Count);
        }

        static async Task Main(string[] args)
        {
            //new ScriptDetector().AttemptUnpack(new FileStream(@"C:\Users\blabl\source\repos\MDKExtract\bin\Debug\net5.0\extractions\LEVEL8.CMI (LEVEL8.CMD)\4GUNT_3.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite), null!);
            //var x = new ResearchCmiDecoder();
            //x.Decode(new FileStream(@"D:\GOG Games\MDK\TRAVERSE\LEVEL8\LEVEL8.CMI", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            await DecodeAll();
        }
    }
}
