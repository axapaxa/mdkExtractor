using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class DtiPart3Extractor : FixedSizeExtractorBase
    {
        protected override int START_HEADER_SIZE => 4;

        protected override int HEADER_NAME_SIZE => 0;

        protected override int ENTRY_HEADER_SIZE => 16;

        protected override int ENTRY_NAME_SIZE => 8;

        protected override Func<BinaryReader, int?> DecodeRootFileSize => reader => null;

        protected override Action<BinaryReader> SkipUselessRootData => reader => { };

        protected override Func<BinaryReader, (int offset, int? size)> ReadEntryData => reader =>
        {
            var offset = reader.ReadInt32() + 4 - 400;
            reader.ReadInt32();//Useless
            return (offset, size: null);
        };

        protected override Func<MemoryStream, (int offset, int? size, string sectionName), ExtractedModel.Section> TryToGetDataLessSection => (stream, sizes) => null;

        protected override Func<Stream, ExtractedModel, int> SkipLastBytesNumber => (stream, model) => 0;
    }
}
