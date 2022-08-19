using MDKExtract.FolderMetadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.RawFileCompressors
{
    public class ScriptDetector : RawFilePacker
    {
        protected override IEnumerable<(Stream stream, string ext)> Unpack(Stream stream, FullFolderMeta meta)
        {
            using var reader = new BinaryReader(stream, Encoding.Default, true);
            if (TryReadingString(reader) == null) throw new ArgumentException();
            if (TryReadingString(reader) == null) throw new ArgumentException();
            stream.Seek(0, SeekOrigin.Begin);
            var output = ExtractionUtils.GetStreamFromData(stream, (int)stream.Length);
            return new[] { (output as Stream, ".script") };
        }

        public string? TryReadingString(BinaryReader reader)
        {
            var length = reader.ReadByte();
            if (length == 0)
            {
                throw new ArgumentException();
            }
            var body = reader.ReadBytes(length);
            if (body.Last() != 0)
            {
                return null;
            }
            if (length == 1)
            {
                return "";
            }
            if (ExtractionUtils.ScoreStringValidity(body.SkipLast(1).ToArray()) < 0.9f) {
                throw new ArgumentException();
            }
            var bodyDecoded = UTF8Encoding.ASCII.GetString(body);
            return bodyDecoded;
        }
    }
}
