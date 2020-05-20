using System;
using System.IO;
using System.Text;

namespace PlayfulSoftware.Meshes.Hybrid.Editor
{
    public class InvalidMagicNumberException : Exception
    {
        internal InvalidMagicNumberException(string magic, string badMagic)
            : base($"Invalid Magic! expected: {magic}, got: {badMagic}")
        {}
    }

    public static class PLYParser
    {

        struct Context
        {

        }
        private const string kMagic = "ply\r";

        private static void CheckFormatAndThrow(string format)
        {
            var items = format.TrimEnd().Split(new[] {" "}, StringSplitOptions.None);
            if (items.Length != 3)
                throw new Exception($"invalid format length: {items.Length}");
            if (items[0] != "format")
                throw new Exception($"expected 'format', got: '{items[0]}'");
            if (items[1] != "ascii")
                throw new Exception($"expected 'ascii', got: '{items[1]}'");
            if (items[2] != "1.0")
                throw new Exception($"expected '1.0', got: '{items[2]}'");
        }

        private static void CheckMagicNumberAndThrow(string magic)
        {
            if (magic.Length != kMagic.Length)
                throw new Exception("Incorrect formatted file header! (wrong magic bytes count)");
            if (kMagic != magic)
                throw new InvalidMagicNumberException(kMagic, magic);
        }

        public static void Parse(Stream data)
        {
            ParseMagic(data);
            ParseFormat(data);
            var line = ReadLine(data);
            while (line != "")
            {
                var items = line.TrimEnd().Split(new[] {" "}, StringSplitOptions.None);
                switch (items[0])
                {
                    case "comment":
                        break;
                    case "element":
                        break;
                    case "property":
                        break;
                    case "end_header":
                        break;
                    default:
                        throw new Exception($"unexpected element: {items[0]}");
                }
            }
        }

        private static string ReadLine(Stream input)
        {
            using (var output = new MemoryStream())
            {
                var ch = input.ReadByte();
                while (ch != -1 && ch != '\n')
                {
                    output.WriteByte((byte)ch);
                    ch = input.ReadByte();
                }

                return Encoding.ASCII.GetString(output.GetBuffer(), 0, (int)output.Length);
            }
        }

        private static void ParseFormat(Stream input)
        {
            var format = ReadLine(input);
            CheckFormatAndThrow(format);
        }

        private static void ParseMagic(Stream input)
        {
            var magic = ReadLine(input);
            CheckMagicNumberAndThrow(magic);
        }
    }
}