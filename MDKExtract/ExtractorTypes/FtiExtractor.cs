using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class FtiExtractor : FixedSizeExtractorBase
    {
        protected override int START_HEADER_SIZE => 8;

        protected override int HEADER_NAME_SIZE => 0;

        protected override int ENTRY_HEADER_SIZE => 12;

        protected override int ENTRY_NAME_SIZE => 8;

        protected override Action<BinaryReader> SkipUselessRootData => reader => { };

        protected override Func<BinaryReader, (int offset, int? size)> ReadEntryData => reader =>
        {
            var offset = reader.ReadInt32() + 4;
            return (offset, size: null);
        };

        protected override Func<Stream, ExtractedModel, int> SkipLastBytesNumber => (stream, model) => 0;

        
    }
}
