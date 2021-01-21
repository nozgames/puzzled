using System;
using UnityEngine;

namespace Puzzled
{
    [Serializable]
    public struct Cell : IEquatable<Cell>
    {
        public static readonly Cell left = new Cell(-1, 0);
        public static readonly Cell right = new Cell(1, 0);
        public static readonly Cell up = new Cell(0, 1);
        public static readonly Cell down = new Cell(0, -1);
        public static readonly Cell zero = new Cell(0, 0);
        public static readonly Cell invalid = new Cell(-99999, -99999);

        public int x;
        public int y;

        public Cell (int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Cell (Vector2Int v)
        {
            x = v.x;
            y = v.y;
        }

        public static Cell Min(Cell lhs, Cell rhs) => new Cell(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
        public static Cell Max(Cell lhs, Cell rhs) => new Cell(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));

        public static Cell operator + (Cell lhs, Cell rhs) => new Cell(lhs.x + rhs.x, lhs.y + rhs.y);
        public static Cell operator - (Cell lhs, Cell rhs) => new Cell(lhs.x - rhs.x, lhs.y - rhs.y);

        public static Cell operator * (Cell lhs, int rhs) => new Cell(lhs.x * rhs, lhs.y * rhs);

        public Vector2 ToVector2() => new Vector2(x, y);

        public Vector3 ToVector3() => new Vector3(x, 0, y);

        public Vector2Int ToVector2Int() => new Vector2Int(x, y);

        public Vector3Int ToVector3Int() => new Vector3Int(x, y, 0);

        public static bool operator ==(Cell lhs, Cell rhs) => lhs.x == rhs.x && lhs.y == rhs.y;

        public static bool operator !=(Cell lhs, Cell rhs) => lhs.x != rhs.x || lhs.y != rhs.y;

        public bool Equals(Cell other) => x == other.x && y == other.y;

        public override bool Equals(object other) => other.GetType() == typeof(Cell) && Equals((Cell)other);

        public override int GetHashCode() => x + y * 99999;

        public override string ToString() => $"({x},{y})";

        public int DistanceTo(Cell other) => Mathf.Abs(x - other.x) + Mathf.Abs(y - other.y);

        public bool IsAdjacentTo(Cell other) => DistanceTo(other) == 1;

        public Cell Flipped() => new Cell(-x, -y);

        public Cell Direction (Cell to) => new Cell(Mathf.Clamp(to.x - x, -1, 1), Mathf.Clamp(to.y - y, -1, 1));

        public Cell normalized => zero.Direction(this);
    }
}
