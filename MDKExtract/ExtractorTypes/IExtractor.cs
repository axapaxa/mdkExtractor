using System.IO;
using System.Threading.Tasks;

namespace MDKExtract.ExtractorTypes
{
    public interface IExtractor
    {
        public Task<ExtractedModel> Extract(Stream data);
    }
}
