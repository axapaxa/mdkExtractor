using MDKExtract.FolderMetadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract
{
    public static class DetectMetaPiecesInFolderStructure
    {
        private static IEnumerable<IFolderMetaDecoder> Decoers = new List<IFolderMetaDecoder>() { new PaleteDecoder() };

        public static IEnumerable<IFolderMetadata> Decode(Stream stream)
        {
            List<IFolderMetadata> results = new();
            if (stream is null)
                return results;
            foreach(var decoder in Decoers)
            {
                try
                {
                    stream.Position = 0;
                    var decoded = decoder.Decode(stream);
                    if (decoded != null)
                        results.Add(decoded);
                }
                catch { }

            }
            return results;
        }
    }
}
