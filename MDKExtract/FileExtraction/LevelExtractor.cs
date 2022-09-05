using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.FileExtraction
{
    public static class LevelExtractor
    {
        public static void Extract(Stream stream, string baseFilePath, bool is1996Level)
        {
            stream.Position = 0;
            var reader = new BinaryReader(stream);
            var textureNum = reader.ReadUInt32();
            if (textureNum > 40)
                throw new InvalidDataException("Invalid texture num");
            var textures = Enumerable.Range(0, (int)textureNum).Select(x => ExtractionUtils.ReadString(reader, is1996Level ? 16 : 10)).ToList();
            while (stream.Position % 4 != 0) reader.ReadByte();
            var aSectionCnt = reader.ReadUInt32();
            if (aSectionCnt > 5000)
                throw new InvalidDataException("Bad A count");
            var aSectionLength = is1996Level ? 36 : 44;
            foreach (var x in Enumerable.Range(0, (int)(aSectionCnt * aSectionLength))) reader.ReadByte();
            var bSectionCnt = reader.ReadUInt32();
            var bSectionStart = stream.Position;
            stream.Seek(36 * bSectionCnt, SeekOrigin.Current);

            using var modelFs = new StreamWriter(baseFilePath + ".obj", false);
            modelFs.WriteLine("# OBJ Model");

            var vertexNum = reader.ReadUInt32();
            if (vertexNum > 4000)
                throw new InvalidDataException("Invalid vertex count");

            void writeVertex()
            {
                var z = reader.ReadSingle();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                modelFs.WriteLine($"v {x.ToString("R", CultureInfo.InvariantCulture)} {y.ToString("R", CultureInfo.InvariantCulture)} {z.ToString("R", CultureInfo.InvariantCulture)}");
            }

            foreach (var vertNum in Enumerable.Range(0, (int)vertexNum))
            {
                writeVertex();
            }

            var unknownFFcnt = reader.ReadUInt32();
            foreach (var x in Enumerable.Range(0, (int)unknownFFcnt)) reader.ReadByte();
            var endOfData = stream.Position;
            stream.Position = bSectionStart;
            foreach (var vertNum in Enumerable.Range(0, (int)bSectionCnt))
            {
                var i1 = reader.ReadUInt16();
                var i2 = reader.ReadUInt16();
                var i3 = reader.ReadUInt16();
                modelFs.WriteLine($"f {i1 + 1} {i2 + 1} {i3 + 1}");
                var probTexture = reader.ReadUInt16();
                for (var j = 0; j < 7; j++) reader.ReadSingle();
            }
            var diff = stream.Length - endOfData;
            if ((diff > 8) || (diff < 0))
                throw new InvalidDataException($"Uhm, not really end of data? Missing {diff} bytes");
        }
    }
}
