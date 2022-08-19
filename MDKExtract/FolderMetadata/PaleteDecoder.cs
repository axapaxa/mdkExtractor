using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.FolderMetadata
{
    public class PaleteDecoder : IFolderMetaDecoder
    {
        public static IFolderMetadata DecodeAsPallete(byte[] array)
        {
            return new PaleteDecoder().Decode(new MemoryStream(array))!;
        }

        public IFolderMetadata? Decode(Stream data)
        {
            if (data.Length != 768)
                return null;
            data.Position = 0;
            var colorList = new List<Color>();
            var reader = new BinaryReader(data);
            for(var i = 0; i<256; i++)
            {
                colorList.Add(Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()));
            }
            return new PalleteFileData() { Colors = colorList.ToArray() };
        }
    }
}
