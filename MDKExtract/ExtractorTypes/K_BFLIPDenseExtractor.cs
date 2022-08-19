using MDKExtract.FileDivisor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class K_BFLIPDenseExtractor : IExtractor
    {
        public Task<ExtractedModel> Extract(Stream data)
        {
            var allocator = new StreamDivider(data);
            var reader = new BinaryReader(data);
            ExtractedModel model;

            var entries = new List<(string name, int offset, int? count, MemoryStream header)>();
            int fileLength;
            using (allocator.StartAllocation("header"))
            {
                var headerRoot = new UndecodedHeadersReader(data);
                var length = reader.ReadInt32() + 4;
                var lenDiff = data.Length - length;
                if (lenDiff > 3 || lenDiff < 0)
                    throw new ArgumentException("Invalid file: Wrong header");
                fileLength = length;
                
                var fileName = "";
                var length1 = reader.ReadInt32();
                var undecodedHeader = headerRoot.FinishReading();
                foreach (var index in Enumerable.Range(1, length1))
                {
                    var entryHeader = new UndecodedHeadersReader(data);
                    var name = index.ToString();
                    var offset = reader.ReadInt32();
                    if (offset != 0)
                        offset += 4;
                    entries.Add((name, offset, count: null, header: entryHeader.FinishReading()));
                }
                model = new ExtractedModel() { Data = new List<ExtractedModel.Section>(), FileName = fileName, UndecodedHeader = undecodedHeader };
            }
            var headerEnd = (int)data.Position;
            if (entries.Select(x => x.offset).Where(x => x != 0).Min() != headerEnd)
                throw new ArgumentException("Header ended too soon(?)");
            model.Data.AddRange(
                entries
                    .Where(x => x.offset == 0)
                    .Select(x => ExtractionUtils.CreateSection(x.header, x.name, data, null, null)));
            var dataToAdd = new List<ExtractedModel.Section>();
            var previousOffset = fileLength;
            foreach (var entry in entries.Where(x => x.offset != 0).OrderByDescending(x => x.offset))
            {
                var start = entry.offset;
                var length = (int)previousOffset - start;
                var countData = entry.count ?? length;
                int? postData = entry.count is null ? null : length - countData;

                dataToAdd.Add(ExtractionUtils.CreateSection(entry.header, entry.name, data, start, countData, postData));
                previousOffset = start;
            }
            if (previousOffset != headerEnd)
                throw new InvalidOperationException("Internal error, mismatch");
            dataToAdd.Reverse();
            dataToAdd.ForEach(x =>
            {
                if (x.Data == null)
                    return;
                x.Data = DecodeRLE(x!.Data);
            });
            model.Data.AddRange(dataToAdd);
            return Task.FromResult(model);
        }

        private MemoryStream DecodeRLE(Stream source)
        {
            var reader = new BinaryReader(source);
            reader.ReadBytes(8);
            var allStreams = new List<byte[]>();
            List<byte> currentStream = new List<byte>();
            while(true)
            {
                var data = reader.ReadByte();
                if (data == 0xFF)
                {
                    allStreams.Add(currentStream.ToArray());
                    break;
                }
                    
                if (data == 0xFE)
                {
                    allStreams.Add(currentStream.ToArray());
                    currentStream.Clear();
                    continue;
                }

                if (data >= 0x80)
                {
                    var toWrite = reader.ReadByte();
                    currentStream.AddRange(Enumerable.Repeat(toWrite, data - 0x80 + 4));
                    continue;
                }
                var toWrtie = reader.ReadBytes(data + 1);
                currentStream.AddRange(toWrtie);
            }

            if (source.Length != source.Position)
                throw new ArgumentException("RLE oob?");
            var maxStreamSize = allStreams.Select(x => x.Length).OrderByDescending(x => x).First();
            var resultMS = new MemoryStream();
            var writer = new BinaryWriter(resultMS);
            writer.Write((short)maxStreamSize);
            writer.Write((short)allStreams.Count);
            foreach(var line in allStreams)
            {
                var extraBytes = maxStreamSize - line.Length;
                var extra1 = extraBytes / 2;
                writer.Write(line);
                writer.Write(Enumerable.Repeat<byte>(0, extraBytes).ToArray());
            }
            return resultMS;
        }
    }
}
