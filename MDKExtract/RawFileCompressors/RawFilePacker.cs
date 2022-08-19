using MDKExtract.FolderMetadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.RawFileCompressors
{
    public abstract class RawFilePacker
    {
        protected abstract IEnumerable<(Stream stream, string ext)> Unpack(Stream stream, FullFolderMeta meta);

        public IEnumerable<(Stream stream, string ext)> AttemptUnpack(Stream stream, FullFolderMeta meta)
        {
            try
            {
                stream.Position = 0;
                return Unpack(stream, meta);
            } catch
            {
                return Enumerable.Empty<(Stream stream, string ext)>();
            }
        }
    }
}
