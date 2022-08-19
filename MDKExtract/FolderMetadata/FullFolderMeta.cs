using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.FolderMetadata
{
    public class FullFolderMeta
    {
        public List<IFolderMetadata> MetaPieces { get; set; }
        public string FolderName { get; set; }
        public string[] PathParts { get; set; }
    }
}
