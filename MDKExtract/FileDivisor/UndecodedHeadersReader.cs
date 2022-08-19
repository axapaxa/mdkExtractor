using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDKExtract.FileDivisor
{
    public class UndecodedHeadersReader
    {
        private readonly Stream _stream;
        private readonly long _start;
        public UndecodedHeadersReader(Stream stream)
        {
            _stream = stream;
            _start = _stream.Position;
        }

        public MemoryStream FinishReading()
        {
            var end = _stream.Position;
            _stream.Position = _start;
            var result = ExtractionUtils.GetStreamFromData(_stream, (int)end - (int)_start);
            if (end != _stream.Position)
                throw new InvalidOperationException();
            return result;
        }
    }
}
