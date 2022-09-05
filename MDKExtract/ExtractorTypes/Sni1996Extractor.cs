using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class Sni1996Extractor : FixedSizeExtractorBase
    {
        protected override int START_HEADER_SIZE => 8;

        protected override int HEADER_NAME_SIZE => 0;

        protected override int ENTRY_HEADER_SIZE => 24;

        protected override int ENTRY_NAME_SIZE => 12;

        protected override Action<BinaryReader> SkipUselessRootData => reader => { };

        protected override Func<BinaryReader, (int offset, int? size)> ReadEntryData => reader =>
        {
            var unknownBytes = reader.ReadUInt32();
            var offset = reader.ReadInt32() + 4;
            var size = reader.ReadInt32();
            if (size == -1)
                return (offset, size: null);
            return (offset, size);
        };

        
    }
}
