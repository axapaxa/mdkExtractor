using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.RawFileCompressors
{
    public static class EmptyDetector
    {
        public static string? IsEmpty(Stream stream)
        {
            if (stream.Length == 0)
                return ".empty";
            stream.Position = 0;
            while (stream.Position != stream.Length)
            {
                var val = stream.ReadByte();
                if (val != 0)
                    return null;
            }
            return ".empty";
        }
    }
}
