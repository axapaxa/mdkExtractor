using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract
{
    public static class ExtractionUtils
    {
        public static string ReadString(BinaryReader reader, int size)
        {
            return new UTF8Encoding().GetString(reader.ReadBytes(size).Reverse().SkipWhile(x => x == 0).Reverse().ToArray());
        }

        public static float ScoreStringValidity(byte[] data)
        {
            var valid = data.Count(x => x >= 32 & x <= 127);
            return (float)valid / data.Length;
        }

        public static ExtractedModel.Section CreateSection(MemoryStream undecodedHeader, string name, Stream stream, int? offset, int? count, int? postCount = null)
        {
            MemoryStream? data = null;
            MemoryStream? postData = null;
            if (offset.HasValue)
            {
                var currentPos = stream.Position;
                stream.Position = offset.Value;
                data = GetStreamFromData(stream, count!.Value);
                if (postCount.HasValue)
                {
                    postData = GetStreamFromData(stream, postCount!.Value);
                }
                stream.Position = currentPos;
            }
            return new ExtractedModel.Section() { Data = data, Name = name, OriginalOffset = offset ?? 0, Size = count, PostData = postData, UndecodedHeader = undecodedHeader };
        }

        public static MemoryStream GetStreamFromData(Stream source, int readCount)
        {
            if (readCount < 0)
                throw new ArgumentOutOfRangeException("Negative allocation");
            if (readCount > 50_000_000)
                throw new ArgumentOutOfRangeException("Allocation too big");
            var buffer = new byte[readCount];
            var realRead = source.Read(buffer, 0, readCount);
            if (realRead != readCount)
                throw new ArgumentException("Invalid number of bytes read");
            var target = new MemoryStream(buffer, 0, buffer.Length, false, true);
            return target;
        }
    }
}
