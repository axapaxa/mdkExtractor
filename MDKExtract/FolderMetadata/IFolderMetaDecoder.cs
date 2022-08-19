using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.FolderMetadata
{
    public interface IFolderMetaDecoder
    {
        public IFolderMetadata? Decode(Stream data);
    }
}
