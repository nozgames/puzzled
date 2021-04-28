using System;
using System.IO;
using UnityEngine;

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
        public static Cell ReadCell (this BinaryReader reader, int version)
        {
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            var edge = CellEdge.None;
            var system = CellCoordinateSystem.Grid;

            if(version >= 7)
            {
                system = (CellCoordinateSystem)reader.ReadByte();
                if (system == CellCoordinateSystem.Edge || system == CellCoordinateSystem.SharedEdge)
                    edge = (CellEdge)reader.ReadByte();
            }

            return new Cell(system, x, y, edge);
        }

        public static Color ReadColor (this BinaryReader reader)
        {
            return new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        public static Decal ReadDecal (this BinaryReader reader, int version)
        {
            var decal = DatabaseManager.GetDecal(reader.ReadGuid());
            if (version > 1)
                decal.flags = (DecalFlags)reader.ReadInt32();

            if (version > 7)
            {
                decal.rotation = reader.ReadSingle();
                decal.offset = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                decal.scale = reader.ReadSingle();
                decal.smoothness = reader.ReadSingle();
                decal.color = reader.ReadColor();
            } else
                decal.flags |= DecalFlags.AutoColor;

            return decal;
        }
    }
}
