using System;
using UnityEngine;

namespace Puzzled
{
    [Serializable]
    public struct Cell : IEquatable<Cell>
    {
        /// <summary>
        /// Cell edge.
        /// 
        /// Note: Cell edge is serialized so the list should not be reordered or modified in any way
        ///       without also modifying serialization
        /// </summary>
        [Flags]
        public enum Edge
        {
            None,
            North,
            South,
            East,
            West
        }

        public static readonly Cell left = new Cell(-1, 0);
        public static readonly Cell right = new Cell(1, 0);
        public static readonly Cell up = new Cell(0, 1);
        public static readonly Cell down = new Cell(0, -1);
        public static readonly Cell zero = new Cell(0, 0);
        public static readonly Cell invalid = new Cell(-99999, -99999);

        public static readonly Cell west = new Cell(-1, 0);
        public static readonly Cell east = new Cell(1, 0);
        public static readonly Cell north = new Cell(0, 1);
        public static readonly Cell south = new Cell(0, -1);

        public static readonly Cell northWest = new Cell(-1, 1);
        public static readonly Cell southWest = new Cell(-1, -1);
        public static readonly Cell northEast = new Cell(1, 1);
        public static readonly Cell southEast = new Cell(1, -1);

        private static readonly Cell[] edgeToOffset = new[] { zero, north, east, west, south };

        /// <summary>
        /// Returns true if the cell represents an edge
        /// </summary>
        public bool isEdge => edge != Edge.None;

        public int x;
        public int y;
        public Edge edge;

        public Cell (Cell cell, Edge edge)
        {
            this.x = cell.x;
            this.y = cell.y;
            this.edge = edge;
        }

        public Cell (int x, int y, Edge edge = Edge.None)
        {
            this.edge = edge;
            this.x = x;
            this.y = y;
        }

        public Cell (Vector2Int v, Edge edge = Edge.None)
        {
            this.edge = edge;
            x = v.x;
            y = v.y;
        }

        private static readonly Cell[] MinOffset = new Cell[5] { zero, north, zero, east, zero };
        private static readonly Cell[] MaxOffset = new Cell[5] { zero, zero,  south, zero, west };

        private static Cell Min(Cell cell) => new Cell(cell + MinOffset[(int)cell.edge], Edge.None);
        private static Cell Max(Cell cell) => new Cell(cell + MaxOffset[(int)cell.edge], Edge.None);

        public static Cell Min(Cell lhs, Cell rhs)
        {
            lhs = Min(lhs.NormalizeEdge());
            rhs = Min(rhs.NormalizeEdge());
            return new Cell(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Edge.None);
        }

        public static Cell Max(Cell lhs, Cell rhs)
        {
            lhs = Max(lhs.NormalizeEdge());
            rhs = Max(rhs.NormalizeEdge());
            return new Cell(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Edge.None);
        }

        public static Cell operator + (Cell lhs, Cell rhs) => new Cell(lhs.x + rhs.x, lhs.y + rhs.y, lhs.edge);
        public static Cell operator - (Cell lhs, Cell rhs) => new Cell(lhs.x - rhs.x, lhs.y - rhs.y, lhs.edge);

        public static Cell operator * (Cell lhs, int rhs) => new Cell(lhs.x * rhs, lhs.y * rhs, lhs.edge);

        public Vector2 ToVector2() => new Vector2(x, y);

        public Vector3 ToVector3() => new Vector3(x, 0, y);

        public Vector2Int ToVector2Int() => new Vector2Int(x, y);

        public Vector3Int ToVector3Int() => new Vector3Int(x, y, 0);

        public static bool operator ==(Cell lhs, Cell rhs) => lhs.Equals(rhs);

        public static bool operator !=(Cell lhs, Cell rhs) => !lhs.Equals(rhs);

        /// <summary>
        /// Returns a cell that alwasy uses the North and East edges by automatically
        /// converting the South and West edges.
        /// </summary>
        /// <returns>Cell with normalized edge</returns>
        public Cell NormalizeEdge ()
        {
            switch (edge)
            {
                default:
                case Edge.None:
                case Edge.North:
                case Edge.East:
                    return this;

                case Edge.West:
                    return new Cell(x - 1, y, Edge.East);

                case Edge.South:
                    return new Cell(x, y - 1, Edge.North);
            }
        }

        /// <summary>
        /// Normalize the cell for the given layer.  This will remove edges from cells what are on layers that dont have edges as well
        /// as normalizing the edges for those that do.
        /// </summary>
        /// <param name="layer">Layer to normalize for</param>
        /// <returns>Normalized cell</returns>
        public Cell NormalizeEdge(TileLayer layer) => (new Cell(this, TileGrid.IsEdgeLayer(layer) ? edge : Edge.None)).NormalizeEdge();

        /// <summary>
        /// Returns true if the cell is equal to the given cell
        /// </summary>
        /// <param name="other">Cell to compare to</param>
        /// <returns>True if the two cells are equal</returns>
        public bool Equals(Cell other)
        {
            var lhs = this.NormalizeEdge();
            var rhs = other.NormalizeEdge();
            return lhs.edge == rhs.edge && lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public override bool Equals(object other) => other.GetType() == typeof(Cell) && Equals((Cell)other);

        public override int GetHashCode() => x + y * 99999;

        public override string ToString() => $"({x},{y})";

        public int DistanceTo(Cell other) => Mathf.Max(Mathf.Abs(x - other.x),Mathf.Abs(y - other.y));

        public bool IsAdjacentTo(Cell other) => DistanceTo(other) == 1;

        public Cell Flipped() => new Cell(-x, -y);

        public Cell Direction (Cell to) => new Cell(Mathf.Clamp(to.x - x, -1, 1), Mathf.Clamp(to.y - y, -1, 1));

        public Cell normalized => zero.Direction(this);

        /// <summary>
        /// Convert an offset to an edge
        /// </summary>
        /// <param name="offset">Cell offset</param>
        /// <returns></returns>
        public static Edge OffsetToEdge(Cell offset)
        {
            if (offset.x > 0)
                return Edge.East;
            else if (offset.x < 0)
                return Edge.West;
            else if (offset.y < 0)
                return Edge.South;
            else if (offset.y > 0)
                return Edge.North;

            return Edge.None;
        }

        /// <summary>
        /// Convert an edge to a cell offset
        /// </summary>
        /// <param name="edge">Edge</param>
        /// <returns>The cell offset that corresponds to the given edge</returns>
        public static Cell EdgeToOffset(Edge edge) => edgeToOffset[(int)edge];
    }

    public struct CellBounds
    {
        public Cell min;
        public Cell max;

        public CellBounds (Cell min, Cell max)
        {
            this.min = Cell.Min(min, max);
            this.max = Cell.Max(min, max);
        }

        public bool Contains (Cell cell)
        {
            // Handle cell edges on the minimum extents
            cell = cell.NormalizeEdge();
            if (cell.edge == Cell.Edge.East && cell.x == min.x - 1)
                cell.x++;

            if (cell.edge == Cell.Edge.North && cell.y == min.y - 1)
                cell.y++;

            return (cell.x >= min.x && cell.x <= max.x && cell.y >= min.y && cell.y <= max.y);
        }

        public static CellBounds operator +(CellBounds lhs, Cell rhs) => new CellBounds(lhs.min + rhs, lhs.max + rhs);
    }
}
