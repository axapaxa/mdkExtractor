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
    }
}
