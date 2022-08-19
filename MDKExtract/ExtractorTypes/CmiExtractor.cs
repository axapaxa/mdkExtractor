using MDKExtract.FileDivisor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class CmiExtractor : IExtractor
    {
        public Task<ExtractedModel> Extract(Stream data)
        {
            var allocator = new StreamDivider(data);
            var reader = new BinaryReader(data);
            ExtractedModel model;

            var entries = new List<(string name, int offset, MemoryStream header)>();
            using (allocator.StartAllocation("header"))
            {
                var headerRoot = new UndecodedHeadersReader(data);
                var length = reader.ReadInt32() + 4;
                if (length != data.Length)
                    throw new ArgumentException("Invalid file: Wrong header");
                var fileName = ExtractionUtils.ReadString(reader, 12);
                var length2 = reader.ReadInt32() + 8 + 4;
                if (length2 != data.Length)
                    throw new ArgumentException("Invalid file: Secondary header wrong");
                var undecodedHeader = headerRoot.FinishReading();
                foreach (var index1 in Enumerable.Range(1, 4))
                {
                    foreach(var index2 in Enumerable.Range(1, reader.ReadInt32()))
                    {
                        var entryHeader = new UndecodedHeadersReader(data);
                        var strLength = reader.ReadByte();
                        var name = index1+ExtractionUtils.ReadString(reader, strLength);
                        var offset = reader.ReadInt32();
                        if (offset != 0)
                            offset += 4;
                        entries.Add((name, offset, header: entryHeader.FinishReading()));
                    }
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
                    .Select(x => new ExtractedModel.Section() {
                        UndecodedHeader = new MemoryStream(),
                        Data =  null,
                        Name = x.name,
                        OriginalOffset = 0,
                        PostData = new MemoryStream(),
                        Size = 0 }));
            var dataToAdd = new List<ExtractedModel.Section>();
            var previousOffset = data.Position - 12;
            foreach(var entry in entries.Where(x => x.offset != 0).OrderByDescending(x => x.offset))
            {
                var start = entry.offset;
                data.Seek(start, SeekOrigin.Begin);
                var length = (int)previousOffset - start;
                dataToAdd.Add(new ExtractedModel.Section()
                {
                    UndecodedHeader = entry.header,
                    Data = ExtractionUtils.GetStreamFromData(data, length),
                    Name = entry.name,
                    OriginalOffset = entry.offset,
                    PostData = new MemoryStream(),
                    Size = length,
                });
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
