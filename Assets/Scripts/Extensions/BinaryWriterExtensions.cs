using System;
using System.IO;

namespace Puzzled
{
    public static class BinaryWriterExtensions 
    {
        public static void WriteFourCC (this BinaryWriter writer, byte a, byte b, byte c, byte d)
        {
            writer.Write(a);
            writer.Write(b);
            writer.Write(c);
            writer.Write(d);
        }

        public static void WriteFourCC(this BinaryWriter writer, char a, char b, char c, char d)
        {
            writer.Write((byte)a);
            writer.Write((byte)b);
            writer.Write((byte)c);
            writer.Write((byte)d);
        }

        public static void Write(this BinaryWriter writer, Guid guid)
        {
            writer.Write(guid.ToByteArray());
        }

        public static void Write(this BinaryWriter writer, Cell cell)
        {
            writer.Write(cell.x);
            writer.Write(cell.y);
            writer.Write((byte)cell.system);

            if (cell.system == CellCoordinateSystem.Edge || cell.system == CellCoordinateSystem.SharedEdge)
                writer.Write((byte)cell.edge);
        }
    }
}
