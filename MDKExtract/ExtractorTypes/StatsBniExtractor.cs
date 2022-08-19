using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class StatsBniExtractor : FixedSizeExtractorBase
    {
        protected override int START_HEADER_SIZE => 8;

        protected override int HEADER_NAME_SIZE => 0;

        protected override int ENTRY_HEADER_SIZE => 16;

        protected override int ENTRY_NAME_SIZE => 8;

        protected override Action<BinaryReader> SkipUselessRootData => x => {};

        protected override Func<BinaryReader, (int offset, int? size)> ReadEntryData => reader =>
        {
            var alwaysZero = reader.ReadInt32();
            var offset = reader.ReadInt32() + 4;
            return (offset, size: null);
        };
    }
}
