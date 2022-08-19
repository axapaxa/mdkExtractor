using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public abstract class ExtractorBase : IExtractor
    {
        protected abstract Func<BinaryReader, int> MeasureStartHeader { get; }
        protected abstract Func<BinaryReader, int> MeasureEntryHeader { get; }

        protected virtual Func<BinaryReader, int?> DecodeRootFileSize => reader => reader.ReadInt32() + 4;

        protected abstract Func<BinaryReader, string> DecodeRootFileName { get; }
        protected abstract Func<BinaryReader, string> DecodeEntryName { get; }
        protected abstract Action<BinaryReader> SkipUselessRootData { get; }
        protected abstract Func<BinaryReader, (int offset, int? size)> ReadEntryData { get; }
        protected virtual Func<MemoryStream, (int offset, int? size, string sectionName), ExtractedModel.Section?> TryToGetDataLessSection => (stream, decoded) => null;
        protected virtual Func<Stream, ExtractedModel, int> SkipLastBytesNumber => (stream, model) => 0;
        protected virtual Func<BinaryReader, int> ReadEntryNumber => (reader) => reader.ReadInt32();

        protected virtual Func<int, bool> IsDifferenceBetweenDataAndHeadOk => diff => (diff == 0) || (diff == 4);

        public async Task<ExtractedModel> Extract(Stream data)
        {
            data.Position = 0;
            var currentHeadOffset = MeasureStartHeader(new BinaryReader(data));
            var undecodedHeader = ExtractionUtils.GetStreamFromData(data, currentHeadOffset);
            var reader = new BinaryReader(undecodedHeader);
            var expectedFileSize = DecodeRootFileSize(reader);
            int dataLength = (int)data.Length;
            if (expectedFileSize.HasValue)
            {
                dataLength = expectedFileSize.Value;
                var diffBetween = data.Length - dataLength;
                if (diffBetween > 4 || diffBetween < 0) 
                    throw new ArgumentException("Not valid extractable: File size mismatch");          
            }
                
            var name = DecodeRootFileName(reader);
            SkipUselessRootData(reader);
            var entries = ReadEntryNumber(reader);
            if (entries > 400)
                throw new ArgumentException("Not valid model: Too many entries");
            var model = new ExtractedModel() { Data = new List<ExtractedModel.Section>(), FileName = name, UndecodedHeader = undecodedHeader };
            var previousLastOffset = 0;
            var dataLessEntries = new List<ExtractedModel.Section>();
            int firstOffsetFound = 0;

            foreach (var entryNumber in Enumerable.Range(0, entries))
            {
                data.Position = currentHeadOffset;
                var entryHeadSize= MeasureEntryHeader(new BinaryReader(data));
                data.Seek(currentHeadOffset, SeekOrigin.Begin);
                currentHeadOffset += entryHeadSize;
                var header = ExtractionUtils.GetStreamFromData(data, entryHeadSize);
                var entryReader = new BinaryReader(header);
                var entryName = DecodeEntryName(entryReader);
                var (offset, size) = ReadEntryData(entryReader);
                var section = TryToGetDataLessSection(header, (offset, size, sectionName: entryName));
                if (section != null)
                {
                    dataLessEntries.Add(section);
                    continue;
                }
                if (firstOffsetFound == 0)
                {
                    firstOffsetFound = offset;
                }

                if (model.Data.Any())
                {
                    var postSize = offset - previousLastOffset;
                    data.Seek(previousLastOffset, SeekOrigin.Begin);
                    var postData = ExtractionUtils.GetStreamFromData(data, postSize);
                    model.Data.Last().SetExtraData(postData);
                }
                
 
                data.Seek(offset, SeekOrigin.Begin);
                MemoryStream entryBuffer = null;
                if (size.HasValue)
                {
                    entryBuffer = ExtractionUtils.GetStreamFromData(data, size.Value);
                }
                
                previousLastOffset = (int)data.Position;
                model.Data.Add(new ExtractedModel.Section() { Data = entryBuffer, Name = entryName, OriginalOffset = offset, UndecodedHeader = header, Size = size });
            }

            if (!IsDifferenceBetweenDataAndHeadOk(firstOffsetFound - currentHeadOffset))
            {
                model.Data.Add(ExtractionUtils.CreateSection(new MemoryStream(), "first_offset_minus_current_head", data, currentHeadOffset, firstOffsetFound - currentHeadOffset));
            }

            {
                var lastToSkip = SkipLastBytesNumber(data, model);
                var postSize = (int)dataLength - previousLastOffset - lastToSkip;
                data.Seek(previousLastOffset, SeekOrigin.Begin);
                var postData = ExtractionUtils.GetStreamFromData(data, postSize);
                model.Data.Last().SetExtraData(postData);
            }

            model.Data.AddRange(dataLessEntries);
            return model;
        }
    }
}
