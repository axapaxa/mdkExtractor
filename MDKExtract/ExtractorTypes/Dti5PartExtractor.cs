using MDKExtract.FileDivisor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class Dti5PartExtractor : IExtractor
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
                var length = reader.ReadInt32() + 4;
                if (length != data.Length)
                    throw new ArgumentException("Invalid file: Wrong header");
                var fileName = ExtractionUtils.ReadString(reader, 12);
                var length2 = reader.ReadInt32() + 4 + 8;
                if (length2 != data.Length)
                    throw new ArgumentException("Invalid file: Wrong second header");
                var undecodedHeader = headerRoot.FinishReading();
                foreach (var index in Enumerable.Range(1, 5))
                {
                    var entryHeader = new UndecodedHeadersReader(data);
                    var name = "part" + index;
                    var offset = reader.ReadInt32();
                    if (index == 1)
                    {
                        if (offset != 36)
                            throw new ArgumentException("Invalid first entry");
                    }
                    if (offset != 0)
                        offset += 4;
                    entries.Add((name, offset, count: null, header: entryHeader.FinishReading()));
                }
                model = new ExtractedModel() { Data = new List<ExtractedModel.Section>(), FileName = fileName, UndecodedHeader = undecodedHeader };
            }
            var headerEnd = (int)data.Position;
            if (entries.Select(x => x.offset).Where(x => x != 0).Min() != headerEnd)
                throw new ArgumentException("Header ended too soon(?)");
            data.Seek(-12, SeekOrigin.End);
            using (allocator.StartAllocation("end"))
            {
                var fileName = ExtractionUtils.ReadString(reader, 12);
                if (fileName != model.FileName)
                    throw new ArgumentException("Invalid file backup header");
            }
            model.Data.AddRange(
                entries
                    .Where(x => x.offset == 0)
                    .Select(x => ExtractionUtils.CreateSection(x.header, x.name, data, null, null)));
            var dataToAdd = new List<ExtractedModel.Section>();
            var previousOffset = data.Length - 12;
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
