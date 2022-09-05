using MDKExtract.FileExtraction;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MDKExtract.RawFileCompressors
{
    public static class ModelDetector
    {
        public enum ModelType
        {
            Level,
            Single,
            Multi
        }

        public static string? TryExtensionFromStreamAndExtract(Stream stream, string baseFilePath)
        {
            var decoded = TryExtensionFromStream(stream);
            if (decoded != null)
            {
                var (type, hasExtraHead) = decoded.Value;
                var is1996BspFile = baseFilePath.EndsWith(".BSP", StringComparison.InvariantCultureIgnoreCase);
                if (is1996BspFile)
                    type = ModelType.Level;
                try
                {
                    if (type == ModelType.Level)
                        LevelExtractor.Extract(stream, baseFilePath, is1996BspFile);
                    else
                        Model1Extract.Extract(stream, baseFilePath, type, hasExtraHead);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cannot decode " + baseFilePath + $" as {type}: " + e.Message);
                }
                return "."+decoded.Value.type.ToString();
            }
            return null;
        }

        public static (ModelType type, bool hasExtraHead)? TryExtensionFromStream(Stream stream)
        {
            var regex = new Regex("^[a-z0-9A-Z_ -]+$");
            bool IsValidName(string name)
            {
                return regex.IsMatch(name);
            }

            try
            {
                stream.Position = 0;
                var reader = new BinaryReader(stream);
                var dword1 = reader.ReadInt32();
                ModelType? modelHint = null;
                var hasExtraHead = false;
                if (dword1 >= 0 && dword1 <= 1)
                {
                    var nextName = ExtractionUtils.ReadString(reader, 10);
                    if (IsValidName(nextName))
                    {
                        stream.Position = 0; //Revert, previous was texture num
                    } else
                    {
                        hasExtraHead = true;
                        stream.Position = 4; //Single previous is not a valid name, it probably is a type hint
                        if (dword1 == 0)
                            modelHint = ModelType.Single;
                        else
                            modelHint = ModelType.Multi;
                    }
                } else
                {
                    stream.Position = 0;
                }

                var numTexture = reader.ReadInt32();
                if (numTexture <= 0)
                    return null;//No textures or invalid
                if (numTexture > 40)
                    return null;//Too many textures to be plausible
                var nameNextBig = ExtractionUtils.ReadString(reader, 16);
                var nameSize = 10;
                if (IsValidName(nameNextBig))
                {
                    nameSize = 16;
                }
                stream.Position -= 16;
                
                foreach (var x in Enumerable.Range(0, numTexture))
                {
                    var name = ExtractionUtils.ReadString(reader, nameSize);
                    if (!regex.IsMatch(name))
                        return null;
                }
                if ((nameSize == 10) && !modelHint.HasValue)
                {
                    return (ModelType.Level, hasExtraHead);
                }
                if (nameSize != 16)
                    throw new ArgumentException("Mismatched model!");
                var subModelCount = reader.ReadInt32();
                var nextName2 = ExtractionUtils.ReadString(reader, 12);
                var modelResult = IsValidName(nextName2) ? ModelType.Multi : ModelType.Single;
                if (modelHint.HasValue && modelResult != modelHint.Value)
                    throw new ArgumentException("Expected model type mismatch");
                return (modelResult, hasExtraHead);
            }
            catch
            {
                return null;
            }
        }
    }
}
