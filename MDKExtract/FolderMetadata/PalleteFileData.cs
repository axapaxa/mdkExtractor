using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.FolderMetadata
{
    public class PalleteFileData : IFolderMetadata
    {
        public Color[] Colors { get; init; }

        public override bool Equals(object? obj)
        {
            return obj is PalleteFileData data &&
                   EqualityComparer<Color[]>.Default.Equals(Colors, data.Colors);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Colors);
        }
    }
}
