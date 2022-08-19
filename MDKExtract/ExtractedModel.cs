using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract
{
    public class ExtractedModel
    {
        public string FileName { get; init; }
        public MemoryStream UndecodedHeader { get; init; }
        public List<Section> Data { get; init; }

        public Section GetElement(string name)
        {
            return Data.Single(x => x.Name == name);
        }

        public class Section
        {
            public MemoryStream UndecodedHeader { get; init; }
            public string Name { get; set; }

            public int? Size { get; set; }
            public int OriginalOffset { get; set; }
            public MemoryStream? Data { get; set; }
            public MemoryStream? PostData { get; set; }

            public void SetExtraData(MemoryStream extra)
            {
                if (Data is null)
                {
                    if (Size != null)
                        throw new InvalidDataException("Inconsistent model");
                    Data = extra;
                } else
                {
                    if (Size == null)
                        throw new InvalidDataException("Inconsistent model");
                    PostData = extra;
                }
            }
        }
    }
}
