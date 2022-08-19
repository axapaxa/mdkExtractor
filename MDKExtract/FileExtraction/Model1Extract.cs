using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MDKExtract.RawFileCompressors.ModelDetector;

namespace MDKExtract.FileExtraction
{
    public static class Model1Extract
    {
        public static void Extract(Stream stream, string baseFilePath, ModelType type, bool hasExtraHead)
        {
            stream.Position = 0;
            var reader = new BinaryReader(stream);
            if (hasExtraHead)
            {
                var modelType = reader.ReadUInt32();
            }
            bool isType2 = type == ModelType.Multi;
            var textureNum = reader.ReadUInt32();

            var textures = Enumerable.Range(0, (int)textureNum).Select(x => ExtractionUtils.ReadString(reader, 16)).ToList();
            uint loopCount = 1;
            if (isType2)
            {
                loopCount = reader.ReadUInt32();
                if (loopCount > 50)
                    throw new InvalidDataException("Prob bad model (too much loopcount)");
            }
            for (var i =0; i<loopCount; i++)
            {
                var modelName = baseFilePath;
                if (isType2)
                {
                    var persName = ExtractionUtils.ReadString(reader, 12);
                    if (!FileClassifier.IsPlausibleName(persName))
                        throw new ArgumentException("Invalid model name " + persName);
                    modelName += "_" + persName;
                    //Console.WriteLine(modelName);
                    
                }
                if (isType2)
                {
                    var originX = reader.ReadSingle();
                    var originY = reader.ReadSingle();
                    var originZ = reader.ReadSingle();
                }
                var vertexNum = reader.ReadUInt32();
                if (vertexNum > 400)
                    throw new InvalidDataException("prob wrong decoding1");

                using var modelFs = new StreamWriter(modelName + ".obj", false);
                modelFs.WriteLine("# OBJ Model");

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

                var vertexIndiceNum = reader.ReadUInt32();
                if (vertexIndiceNum > 400)
                    throw new InvalidDataException("prob wrong decoding2");
                foreach (var num in Enumerable.Range(0, (int)vertexIndiceNum))
                {
                    var i1 = reader.ReadUInt16();
                    var i2 = reader.ReadUInt16();
                    var i3 = reader.ReadUInt16();
                    modelFs.WriteLine($"f {i1 + 1} {i2 + 1} {i3 + 1}");
                    var probTexture = reader.ReadUInt16();
                    for (var j = 0; j < 7; j++) reader.ReadSingle();
                }
                if (isType2)
                {
                    foreach (var num in Enumerable.Range(0, 6)) reader.ReadSingle();//Unknown right before model ends..?
                }
            }

            foreach (var num in Enumerable.Range(0, 6)) reader.ReadSingle();//Unknown right before model ends..?
            if (!isType2)
            {
                var unknownExtra = reader.ReadUInt32();
                foreach (var num in Enumerable.Range(0, (int)(unknownExtra * 3))) reader.ReadSingle();
                if (stream.Length != stream.Position)
                    throw new InvalidDataException($"Unknown end of type1 model :( {stream.Length - stream.Position} more bytes");
            }
        }
    }
}
