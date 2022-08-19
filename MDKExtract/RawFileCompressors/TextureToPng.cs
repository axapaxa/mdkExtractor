using MDKExtract.DimensionDetector;
using MDKExtract.FolderMetadata;
using MDKExtract.PaleteExtraction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.RawFileCompressors
{
    public class TextureToPng : RawFilePacker
    {
        protected override IEnumerable<(Stream stream, string ext)> Unpack(Stream stream, FullFolderMeta meta)
        {
            if (stream.Length < 10)
            {
                return Enumerable.Empty<(Stream stream, string ext)>();//Not valid texture
            }
            if (stream is MemoryStream ms)
            {
                stream = ms;
            } else
            {
                stream.Position = 0;
                var ms1 = new MemoryStream((int)stream.Length);
                stream.CopyTo(ms1);
                stream = ms1;
            }
            stream.Position = 0;
            int maxX;
            int maxY;
            int startOffset = 0;
            var palletesToTry = meta.MetaPieces.OfType<PalleteFileData>().Where(x => x.Colors.Length == 256);
            switch (stream.Length)
            {
                case 650_880:
                    maxX = 360;
                    maxY = 1808;
                    break;
                case 649_440:
                    maxX = 360;
                    maxY = 1804;
                    break;
                case 307_200:
                    maxX = 480;
                    maxY = 640;
                    break;
                default:
                    var found = false;
                    var reader = new BinaryReader(stream);
                    maxY = reader.ReadInt16();
                    maxX = reader.ReadInt16();
                    if (maxX * maxY == stream.Length - 4)
                    {
                        found = true;
                        startOffset = 4;
                    }
                    if (stream.Length > 1000 && !found)
                    {
                        stream.Position = 0x300;
                        maxY = reader.ReadInt16();
                        maxX = reader.ReadInt16();
                        if (maxX * maxY == stream.Length - 0x304)
                        {
                            found = true;
                            startOffset = 0x304;
                            stream.Position = 0;
                            palletesToTry = palletesToTry.Append((PalleteFileData)PaleteDecoder.DecodeAsPallete(reader.ReadBytes(0x300)));
                        }
                    }
                    if (!found)
                    {
                        stream.Position = 0;
                        var count = reader.ReadUInt32();
                        maxY = reader.ReadInt16();
                        maxX = reader.ReadInt16() * (int)count;
                        startOffset = 8;
                        found = (maxX * maxY == stream.Length - 8);
                    }
                    if (!found)
                    {
                        return Enumerable.Empty<(Stream stream, string ext)>();//Not valid texture
                    }
                    
                    break;
            }

            
            if (!palletesToTry.Any())
                throw new InvalidDataException("Found image, but no palette to decode it");

            /*stream.Seek(4, SeekOrigin.Begin);
            var bestMatch = DetectDimension.DecodeXDimension(stream, new int[] { maxX, maxY });
            if (bestMatch == maxY)
            {
                var tmp = maxX;
                maxX = maxY;
                maxY = tmp;
            }*/

            var decoded = palletesToTry.AsParallel().Select(pallete =>
            {
                var array = ((MemoryStream)stream).GetBuffer();
                var offset = startOffset;
                using (Bitmap b = new Bitmap(maxY, maxX))
                {
                    for (var x = 0; x < maxX; x++)
                    {
                        for (var y = 0; y < maxY; y++)
                        {
                            b.SetPixel(y, x, pallete.Colors[array[offset++]]);
                        }
                    }
                    var ms = new MemoryStream();
                    b.Save(ms, ImageFormat.Png);
                    return ms as Stream;
                }
            });
            return decoded.Select(x => (stream:x , ext: ".png")).ToList();
        }
    }
}
