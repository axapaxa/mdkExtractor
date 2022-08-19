using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public abstract class FixedSizeExtractorBase : ExtractorBase
    {
        protected abstract int START_HEADER_SIZE { get; }

        protected abstract int HEADER_NAME_SIZE { get; }

        protected abstract int ENTRY_HEADER_SIZE { get; }

        protected abstract int ENTRY_NAME_SIZE { get; }

        protected override Func<BinaryReader, string> DecodeRootFileName => reader => ExtractionUtils.ReadString(reader, HEADER_NAME_SIZE);

        protected override Func<BinaryReader, string> DecodeEntryName => reader => ExtractionUtils.ReadString(reader, ENTRY_NAME_SIZE);

        protected override Func<BinaryReader, int> MeasureStartHeader => reader => START_HEADER_SIZE;
        protected override Func<BinaryReader, int> MeasureEntryHeader => reader => ENTRY_HEADER_SIZE;
    }
}
