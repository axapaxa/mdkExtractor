using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    class Mti1996Extractor : FixedSizeExtractorBase
    {

        protected override int START_HEADER_SIZE => 8;

        protected override int HEADER_NAME_SIZE => 0;

        protected override int ENTRY_HEADER_SIZE => 24;

        protected override int ENTRY_NAME_SIZE => 8;

        protected override Action<BinaryReader> SkipUselessRootData => reader => { };

        protected override Func<BinaryReader, (int offset, int? size)> ReadEntryData => reader =>
        {
            reader.ReadBytes(10);
            var badMagic = (reader.ReadByte() != 0x60);
            badMagic = badMagic || (reader.ReadByte() != 0x40);
            var offset = reader.ReadInt32();
            if (offset == 0 && badMagic)
            {
                return (0, null);
            }
            if (badMagic)
                throw new ArgumentException();
            return (offset: offset + 4, null);
        };

        protected override Func<MemoryStream, (int offset, int? size, string sectionName), ExtractedModel.Section> TryToGetDataLessSection => (stream, sizes) =>
        {
            if (sizes.offset == 0)
                return new ExtractedModel.Section() { Data = null, Name = sizes.sectionName, OriginalOffset = 0, PostData = null, UndecodedHeader = stream, Size = null };
            return null;
        };
    }
}
