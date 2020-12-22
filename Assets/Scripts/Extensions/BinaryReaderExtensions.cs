using System;
using System.IO;

namespace Puzzled
{
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// Read a FourCC code from the stream and compare it to the expected value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="a">First byte of the fourCC code</param>
        /// <param name="b">Second byte of the fourCC code</param>
        /// <param name="c">Third byte of the fourCC code</param>
        /// <param name="d">Fourth byte of the fourCC code</param>
        /// <returns>True if the FourceCC code matches</returns>
        public static bool ReadFourCC (this BinaryReader reader, char a, char b, char c, char d)
        {
            var aa = reader.ReadByte();
            var bb = reader.ReadByte();
            var cc = reader.ReadByte();
            var dd = reader.ReadByte();
            return aa == a && bb == b && cc == c && dd == d;
        }

        /// <summary>
        /// Read a guid
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <returns>Guid</returns>
        public static Guid ReadGuid (this BinaryReader reader)
        {
            return new Guid(reader.ReadBytes(16));
        }

        /// <summary>
        /// Read a cell
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <returns>Cell</returns>
        public static Cell ReadCell (this BinaryReader reader)
        {
            return new Cell(reader.ReadInt32(), reader.ReadInt32());
        }
    }
}
