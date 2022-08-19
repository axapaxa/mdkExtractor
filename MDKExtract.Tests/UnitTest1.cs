using FluentAssertions;
using MDKExtract.RawFileCompressors;
using System;
using System.IO;
using Xunit;

namespace MDKExtract.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void DecodesCDANT_4AsModel()
        {
            using var fs = new FileStream(@"testCases/CDANT_4.weakModel", FileMode.Open, FileAccess.Read);
            var ext = ModelDetector.TryExtensionFromStream(fs);
            ext.Should().Be(".model");
        }

        [Fact]
        public void Decodes_2XPGUNWith1AsWeakModel()
        {
            using var fs = new FileStream(@"testCases/2XPGUN.dat", FileMode.Open, FileAccess.Read);
            var ext = ModelDetector.TryExtensionFromStream(fs);
            ext.Should().Be(".weakModel");
        }
    }
}
