using MDKExtract.FileDivisor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class Part2Extractor : IExtractor
    {
        public Task<ExtractedModel> Extract(Stream data)
        {
            var allocator = new StreamDivider(data);
            var reader = new BinaryReader(data);
            ExtractedModel model;

            var entries = new List<(string name, int offset, int? count, MemoryStream header)>();
            using (allocator.StartAllocation("header"))
            {
                var headerRoot = new UndecodedHeadersReader(data);
                var length = reader.ReadInt32();
                if (length != data.Length)
                    throw new ArgumentException("Invalid file: Wrong header");
                var fileName = "";
                var length1 = reader.ReadInt32();
                var length2 = reader.ReadInt32();
                var length3 = reader.ReadInt32();
                var undecodedHeader = headerRoot.FinishReading();
                foreach (var index in Enumerable.Range(1, length1))
                {
                    var entryHeader = new UndecodedHeadersReader(data);
                    var name = "1"+ ExtractionUtils.ReadString(reader, 8);
                    var offset = reader.ReadInt32();
                    if (offset != 0)
                        offset += 4;
                    entries.Add((name, offset, count: null, header: entryHeader.FinishReading()));
                }
                foreach (var index in Enumerable.Range(1, length2))
                {
                    var entryHeader = new UndecodedHeadersReader(data);
                    var name = "2" + ExtractionUtils.ReadString(reader, 8);
                    var offset = reader.ReadInt32();
                    if (offset != 0)
                        offset += 4;
                    entries.Add((name, offset, count: null, header: entryHeader.FinishReading()));
                }
                foreach (var index in Enumerable.Range(1, length3))
                {
                    var entryHeader = new UndecodedHeadersReader(data);
                    var name = "3" + ExtractionUtils.ReadString(reader, 8);
                    reader.ReadBytes(8);
                    var offset = reader.ReadInt32();
                    if (offset != 0)
                        offset += 4;
                    var count = reader.ReadInt32() - 4;
                    entries.Add((name, offset, count, header: entryHeader.FinishReading()));
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
            var previousOffset = data.Length;
            foreach(var entry in entries.Where(x => x.offset != 0).OrderByDescending(x => x.offset))
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
            model.Data.AddRange(dataToAdd);
            return Task.FromResult(model);
        }
    }
}
