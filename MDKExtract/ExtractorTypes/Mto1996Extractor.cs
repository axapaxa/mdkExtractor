using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    class Mto1996Extractor : FixedSizeExtractorBase
    {

        protected override int START_HEADER_SIZE => 4;

        protected override int HEADER_NAME_SIZE => 0;

        protected override int ENTRY_HEADER_SIZE => 12;

        protected override int ENTRY_NAME_SIZE => 8;

        protected override Action<BinaryReader> SkipUselessRootData => reader => { };

        protected override Func<BinaryReader, (int offset, int? size)> ReadEntryData => reader => (offset: reader.ReadInt32(), null);

        protected override Func<BinaryReader, int?> DecodeRootFileSize => reader => null;
    }
}
