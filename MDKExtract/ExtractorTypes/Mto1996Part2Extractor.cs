using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    class Mto1996Part2Extractor : FixedSizeExtractorBase
    {

        protected override int START_HEADER_SIZE => 8;

        protected override int HEADER_NAME_SIZE => 0;

        protected override int ENTRY_HEADER_SIZE => 24;

        protected override int ENTRY_NAME_SIZE => 8;

        protected override Action<BinaryReader> SkipUselessRootData => reader => { };

        protected override Func<BinaryReader, (int offset, int? size)> ReadEntryData => reader =>
        {
            reader.ReadBytes(10);
            if (reader.ReadByte() != 0x60) throw new ArgumentException();
            if (reader.ReadByte() != 0x40) throw new ArgumentException();
            return (offset: reader.ReadInt32() + 4, null);
        };
    }
}
