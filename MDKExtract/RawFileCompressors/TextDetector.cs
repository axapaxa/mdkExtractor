using MDKExtract.FolderMetadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.RawFileCompressors
{
    public class TextDetector : RawFilePacker
    {
        protected override IEnumerable<(Stream stream, string ext)> Unpack(Stream stream, FullFolderMeta meta)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(stream, Encoding.ASCII, true);
            var body = reader.ReadBytes((int)stream.Length);
            if (body.Length > 500)
            {
                throw new ArgumentException();
            }
            if (ExtractionUtils.ScoreStringValidity(body) < 0.8f)
            {
                throw new ArgumentException();
            }
            return new[] { (new MemoryStream(body) as Stream, ".text") };
        }
    }
}
