using MDKExtract.ExtractorTypes;
using MDKExtract.FolderMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MDKExtract.PaleteExtraction
{
    public static class PalleteAppender
    {
        public static IEnumerable<IFolderMetadata> GetPalletes(ExtractedModel model, IExtractor usedExtractor, FullFolderMeta meta)
        {
            if (usedExtractor is _3PartExtractor)
            {
                var regex = new Regex("LEVEL([0-9])O\\.MAT");
                var palletePart = model.GetElement("part3small").Data!;
                var match = regex.Match(meta.PathParts.First())!;
                var rootPallete = GlobalPalletes.GetMainPallettes().LevelMtoPalette[int.Parse(match.Groups[1].Value)];
                var resultPallete = rootPallete.ToArray();
                palletePart.ToArray().CopyTo(resultPallete, 0xC0);
                return new IFolderMetadata[] { PaleteDecoder.DecodeAsPallete(resultPallete) };
            }

            if (usedExtractor is MtiExtractor)
            {
                var regex = new Regex("LEVEL([0-9])S\\.MAT");
                var match = regex.Match(meta.PathParts.First())!;
                if (match.Success)
                {
                    var rootPallete = GlobalPalletes.GetMainPallettes().LevelMtoPalette[int.Parse(match.Groups[1].Value)];
                    var resultPallete = rootPallete.ToArray();
                    return new IFolderMetadata[] { PaleteDecoder.DecodeAsPallete(resultPallete) };
                }
            }

            if (usedExtractor is Dti5PartExtractor)
            {
                var regex = new Regex("LEVEL([0-9])\\.DAT");
                var match = regex.Match(meta.PathParts.First())!;
                var rootPallete = GlobalPalletes.GetMainPallettes().LevelMtoPalette[int.Parse(match.Groups[1].Value)];
                var resultPallete = rootPallete.ToArray();
                return new IFolderMetadata[] { PaleteDecoder.DecodeAsPallete(resultPallete) };
            }

            if (usedExtractor is MtiExtractor)
            {
                var regex = new Regex("FALL3D_([0-9])\\.MAT");
                var match = regex.Match(meta.PathParts.First())!;
                if (match.Success)
                {
                    var resultPallete = GlobalPalletes.GetMainPallettes().FallPalette[int.Parse(match.Groups[1].Value)];
                    return new IFolderMetadata[] { PaleteDecoder.DecodeAsPallete(resultPallete) };
                }
                else
                if (meta.PathParts.First() == "STATS.MAT")
                {
                    return new IFolderMetadata[] { PaleteDecoder.DecodeAsPallete(GlobalPalletes.GetMainPallettes().StatsPalette) };
                }
                else if (meta.PathParts.First() == "STREAM.MAT")
                {
                    return new IFolderMetadata[] { PaleteDecoder.DecodeAsPallete(GlobalPalletes.GetMainPallettes().StreamPalette) };
                }
            }

            if (usedExtractor is StatsBniExtractor)
            {
                if (meta.PathParts.First() == "TRAVSPRT.BNI")
                {
                    return new IFolderMetadata[] { PaleteDecoder.DecodeAsPallete(GlobalPalletes.GetMainPallettes().LevelMtoPalette[7]) };
                }
            }

            if (usedExtractor is DefaultSniExtractor)
            {
                var regex = new Regex("LEVEL([0-9])S\\.SND");
                var match = regex.Match(meta.PathParts.First())!;
                if (match.Success)
                {
                    var rootPallete = GlobalPalletes.GetMainPallettes().LevelMtoPalette[int.Parse(match.Groups[1].Value)];
                    var resultPallete = rootPallete.ToArray();
                    return new IFolderMetadata[] { PaleteDecoder.DecodeAsPallete(resultPallete) };
                }
            }

            return Enumerable.Empty<IFolderMetadata>();
        }
    }
}
