using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.DimensionDetector
{
    public static class DetectDimension
    {
        public static int DecodeXDimension(Stream source, ICollection<int> candidates)
        {
            var memory = new MemoryStream();
            source.CopyTo(memory);
            var bestCandidates = candidates.AsParallel().Select(x => (x: x, score: ScorePattern(memory.GetBuffer(), x, (int)Math.Floor((double)memory.Length / x))))
                .OrderByDescending(x => x.score).First();
            return bestCandidates.x;
        }

        public static float ScorePattern(byte[] data, int maxX, int maxY)
        {
            Func<int, int, byte> readByte = (x, y) =>
            {
                if (x > maxX) x = maxX; else if (x < 0) x = 0;
                if (y > maxY) y = maxY; else if (y < 0) y = 0;
                return data[x * maxY + y];
            };

            int score = 0;
            for(var x = 0; x < maxX; x++)
            {
                for (var y=0; y<maxY; y++)
                {
                    var sourcePixel = readByte(x, y);
                    var targetPixels = new[]
                    {
                        readByte(x+1, y),
                        readByte(x-1, y),
                        readByte(x+1, y+1),
                        readByte(x+1, y-1),
                        readByte(x-1, y+1),
                        readByte(x-1, y-1),
                    };
                    var numMatches = targetPixels.Count(x => x == sourcePixel);
                    if (numMatches != targetPixels.Length)
                    {
                        score = score + numMatches;
                    }
                }
            }
            return score;
        }
    }
}
