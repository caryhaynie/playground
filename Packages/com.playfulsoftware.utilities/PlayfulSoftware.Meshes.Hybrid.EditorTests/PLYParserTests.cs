using System.IO;
using System.Text;
using NUnit.Framework;
using PlayfulSoftware.Meshes.Hybrid.Editor;

namespace Tests
{
    public class PLYParserTests
    {
        // test data for magic tests
        const string invalidMagic = "err\r\nformat ascii 1.0\r\n";
        const string validMagic = "ply\r\nformat ascii 1.0\r\n";
        // test dat for format tests
        const string invalidFormat = "ply\r\nformat ascii 0.1\r\n";
        const string validFormat = "ply\r\nformat ascii 1.0\r\n";

        private Stream TestDataAsStream(string data)
            => new MemoryStream(Encoding.ASCII.GetBytes(data));


        [Test]
        public void ParseThrowsExceptionIfFileIsEmpty()
        {
            using (var stream = new MemoryStream())
                Assert.That(() => PLYParser.Parse(stream), Throws.Exception);
        }

        [Test]
        public void ParseThrowsExceptionIfFileHasInvalidMagic()
        {
            using (var stream = TestDataAsStream(invalidMagic))
                Assert.That(() => PLYParser.Parse(stream), Throws.InstanceOf<InvalidMagicNumberException>());
        }

        [Test]
        public void ParseDoesNotThrowIfFileHasValidMagic()
        {
            using (var stream = TestDataAsStream(validMagic))
                Assert.That(() => PLYParser.Parse(stream), Throws.Nothing);
        }

        [Test]
        public void ParseThrowsExceptionIfFileHasInvalidFormat()
        {
            using (var stream = TestDataAsStream(invalidFormat))
                Assert.That(() => PLYParser.Parse(stream), Throws.Exception);
        }

        [Test]
        public void ParseDoesNotThrowIfFileHasValidFormat()
        {
            using (var stream = TestDataAsStream(validFormat))
                Assert.That(() => PLYParser.Parse(stream), Throws.Nothing);
        }
    }
}