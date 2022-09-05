using MDKExtract.ExtractorTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.PaleteExtraction
{
    public class GlobalPalletes
    {
        private static GlobalPalletes? _singleton;
        public static GlobalPalletes GetMainPallettes()
        {
            if (_singleton == null)
                _singleton = new GlobalPalletes();
            return _singleton;
        }

        public Dictionary<int, byte[]> LevelMtoPalette;
        public Dictionary<int, byte[]> FallPalette;
        public byte[] StreamPalette;
        public byte[] StatsPalette;
        public GlobalPalletes()
        {
            LevelMtoPalette = new Dictionary<int, byte[]>();
            FallPalette = new Dictionary<int, byte[]>();
        }

        public async Task ReadStatsPalette(DirectoryInfo dir)
        {
            var filesToParse = dir.GetFiles("STATS.BNI", new EnumerationOptions() { RecurseSubdirectories = true });
            if (!filesToParse.Any())
                return;
            using var fs = new FileStream(filesToParse.Single().FullName, FileMode.Open, FileAccess.Read);
            var results = await new StatsBniExtractor().Extract(fs);
            var levelData = results.GetElement("PAL").Data!;
            if (levelData.Length != 0x300)
                throw new ArgumentException("Invalid PAL file length?");
            var reader = new BinaryReader(levelData);
            StatsPalette = reader.ReadBytes(0x300);
        }

        public async Task ReadStreamPalette(DirectoryInfo dir)
        {
            var filesToParse = dir.GetFiles("STREAM.BNI", new EnumerationOptions() { RecurseSubdirectories = true });
            if (!filesToParse.Any())
                return;
            using var fs = new FileStream(filesToParse.Single().FullName, FileMode.Open, FileAccess.Read);
            var results = await new StatsBniExtractor().Extract(fs);
            var levelData = results.GetElement("PAL").Data!;
            if (levelData.Length != 0x300)
                throw new ArgumentException("Invalid PAL file length?");
            var reader = new BinaryReader(levelData);
            StreamPalette = reader.ReadBytes(0x300);
        }

        public async Task ReadMtoFiles(DirectoryInfo dir)
        {
            var filesToParse = dir.GetFiles("LEVEL?.DTI", new EnumerationOptions() { RecurseSubdirectories = true });
            var parsedFileTasks = filesToParse.Select(async x =>
            {
                using var fs = new FileStream(x.FullName, FileMode.Open, FileAccess.Read);
                var results = await new Dti5PartExtractor().Extract(fs);
                var key = results.FileName[5] - '0';
                var levelData = results.GetElement("part4").Data!;
                var reader = new BinaryReader(levelData);
                var head = reader.ReadInt32();
                //if (head != 112)
                //    throw new ArgumentException("Invalid DTI file?");
                if (levelData.Length != 0x304)
                    throw new ArgumentException("Invalid DTI file length?");
                return (key, data: reader.ReadBytes(0x300));
            });

            foreach(var x in (await Task.WhenAll(parsedFileTasks)))
            {
                LevelMtoPalette.Add(x.key, x.data);
            }
        }

        public async Task ReadFallFiles(DirectoryInfo dir)
        {
            var filesToParse = dir.GetFiles("FALL3D.BNI", new EnumerationOptions() { RecurseSubdirectories = true });
            if (!filesToParse.Any())
                return;
            using var fs = new FileStream(filesToParse.Single().FullName, FileMode.Open, FileAccess.Read);
            var results = await new StatsBniExtractor().Extract(fs);
            var parsedFileTasks = results.Data.Where(x => x.Name.StartsWith("FALLP") && x.Name.Length == 6).Select(async x =>
            {
                var key = x.Name[5] - '0';
                var levelData = x.Data!;
                if (levelData.Length != 0x300)
                    throw new ArgumentException("Invalid PAL file length?");
                var reader = new BinaryReader(levelData);
                return (key, data: reader.ReadBytes(0x300));
            });

            foreach (var x in (await Task.WhenAll(parsedFileTasks)))
            {
                FallPalette.Add(x.key, x.data);
            }
        }
    }
}
