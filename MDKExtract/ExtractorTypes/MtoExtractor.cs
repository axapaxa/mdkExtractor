using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class MtoExtractor : FixedSizeExtractorBase
    {
        protected override int START_HEADER_SIZE => 24;

        protected override int HEADER_NAME_SIZE => 12;

        protected override int ENTRY_HEADER_SIZE => 12;

        protected override int ENTRY_NAME_SIZE => 8;

        protected override Action<BinaryReader> SkipUselessRootData => reader => reader.ReadInt32();

        protected override Func<BinaryReader, (int offset, int? size)> ReadEntryData => reader =>
        {
            var offset = reader.ReadInt32() + 4;
            return (offset, size: null);
        };

        protected override Func<MemoryStream, (int offset, int? size, string sectionName), ExtractedModel.Section> TryToGetDataLessSection => (stream, sizes) =>
        {
            if (sizes.offset == 4)
                return new ExtractedModel.Section() { Data = null, Name = sizes.sectionName, OriginalOffset = 0, PostData = null, UndecodedHeader = stream, Size = null };
            return null;
        };

        protected override Func<Stream, ExtractedModel, int> SkipLastBytesNumber => (stream, model) =>
        {
            stream.Seek(-12, SeekOrigin.End);
            var readName = ExtractionUtils.ReadString(new BinaryReader(stream), 12);
            if (readName != model.FileName)
                throw new ArgumentException("File name mismatch in first and last header");
            return 12;
        };
    }
}
