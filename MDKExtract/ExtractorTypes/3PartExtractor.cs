using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public class _3PartExtractor : IExtractor
    {
        public async Task<ExtractedModel> Extract(Stream data)
        {
            var undecodedHeader = ExtractionUtils.GetStreamFromData(data, 12);
            var reader = new BinaryReader(undecodedHeader);
            data.Position = 0;
            var endOfFirstPart = reader.ReadInt32();
            var partThreePre = reader.ReadInt32();
            var partThreeActual = reader.ReadInt32();
            if (partThreeActual != partThreePre + 0x150)
            {
                throw new ArgumentException("Not valid 3part file");
            }

            var model = new ExtractedModel() { Data = new(), UndecodedHeader = undecodedHeader, FileName = "" };
            data.Position = 12;
            var data1Size = endOfFirstPart - 12;
            var data1 = ExtractionUtils.GetStreamFromData(data, data1Size);
            model.Data.Add(new ExtractedModel.Section() { Name = "part1_material", Data = data1, OriginalOffset = 12, PostData = new MemoryStream(), Size = data1Size, UndecodedHeader = new MemoryStream() });
            
            var data2Offset = (int)data.Position;
            var realReader = new BinaryReader(data);
            var data2RealSize = realReader.ReadInt32();
            data.Seek(-4, SeekOrigin.Current);
            if (data2RealSize > partThreePre - data2Offset)
                throw new ArgumentException("Invalid real size of part2");
            var data2PostSize = partThreePre - data2Offset - data2RealSize;
            var data2 = ExtractionUtils.GetStreamFromData(data, data2RealSize);
            var data2Post = ExtractionUtils.GetStreamFromData(data, data2PostSize);
            model.Data.Add(new ExtractedModel.Section() { Name = "part2", Data = data2, OriginalOffset = data2Offset, PostData = data2Post, Size = data2RealSize, UndecodedHeader = new MemoryStream() });

            if (partThreePre != data.Position)
                throw new InvalidOperationException("Internal failure");
            var data3Size = 0x150;
            var data3 = ExtractionUtils.GetStreamFromData(data, data3Size);
            model.Data.Add(new ExtractedModel.Section() { Name = "part3small", Data = data3, OriginalOffset = partThreePre, PostData = new MemoryStream(), Size = data3Size, UndecodedHeader = new MemoryStream() });
        
            if (partThreeActual != data.Position)
                throw new InvalidOperationException("Internal failure");
            var data4Size = (int)data.Length - (int)data.Position;
            var data4 = ExtractionUtils.GetStreamFromData(data, data4Size);
            model.Data.Add(new ExtractedModel.Section() { Name = "part4", Data = data4, OriginalOffset = partThreeActual, PostData = new MemoryStream(), Size = data4Size, UndecodedHeader = new MemoryStream() });
            return model;
        }
    }
}
