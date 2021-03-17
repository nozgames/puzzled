using System;
using UnityEngine;

namespace Puzzled
{
    public enum CellCoordinateSystem
    {
        Invalid,

        /// <summary>
        /// 2D grid represented by X,Y coordinate
        /// </summary>
        Grid,

        /// <summary>
        /// Edge inside a position within a 2D grid 
        /// </summary>
        Edge,

        /// <summary>
        /// Edge between two grid positions (Uses North and East edges)
        /// </summary>
        SharedEdge
    }

    // NOTE: keep this order, None needs to be first too
    public enum CellEdge
    {
        None,
        North,
        East,
        South,
        West
    }

    public enum CellEdgeSide
    {
        Min,
        Max
    }

    public struct Cell
    {
        public static readonly Cell invalid = new Cell(CellCoordinateSystem.Invalid, 0, 0, CellEdge.None);
        public static readonly Cell zero = new Cell(0, 0);

        public CellCoordinateSystem _system;
        public CellEdge _edge;
        public int _x;
        public int _y;

        public CellCoordinateSystem system => _system;
        public CellEdge edge => _edge;
        public int x => _x;
        public int y => _y;

        public Cell (CellCoordinateSystem system)
        {
            _system = system;
            _x = 0;
            _y = 0;
            _edge = (_system == CellCoordinateSystem.Edge || _system == CellCoordinateSystem.SharedEdge) ? CellEdge.North : CellEdge.None;
        }

        public Cell (int x, int y)
        {
            _x = x;
            _y = y;
            _system = CellCoordinateSystem.Grid;
            _edge = CellEdge.None;
        }

        public Cell (int x, int y, CellEdge edge)
        {
            _x = x;
            _y = y;
            _system = CellCoordinateSystem.Edge;
            _edge = edge;
        }

        public Cell(Cell cell, CellEdge edge) : this(cell.system, cell.x, cell.y, edge) { }

        public Cell(CellCoordinateSystem system, Cell cell, CellEdge edge) : this(system, cell.x, cell.y, edge) { }

        public Cell (CellCoordinateSystem system, int x, int y, CellEdge edge)
        {
            _system = system;

            if (system == CellCoordinateSystem.Invalid)
            {
                _x = 0;
                _y = 0;
                _edge = CellEdge.None;
                return;
            }

            _x = x;
            _y = y;
            _edge = edge;

            // Clear the edge if using grid
            if (_system == CellCoordinateSystem.Grid)
                _edge = CellEdge.None;

            // Shared edges are always represented by the north or east edge of a given tile
            else if(_system == CellCoordinateSystem.SharedEdge)
            {
                if (_edge == CellEdge.South)
                {
                    _y--;
                    _edge = CellEdge.North;
                } 
                else if (_edge == CellEdge.West)
                {
                    _x--;
                    _edge = CellEdge.East;
                }
            }
        }

        public static Cell Min (Cell cell)
        {
            if(cell.system == CellCoordinateSystem.SharedEdge)
                return new Cell(
                    cell.x + (cell.edge == CellEdge.East ? 1 : 0), 
                    cell.y + (cell.edge == CellEdge.North ? 1 : 0));

            return new Cell(cell.x, cell.y);
        }

        public static Cell Max(Cell cell) => new Cell(cell.x, cell.y);

        public static Cell Min(Cell lhs, Cell rhs)
        {
            lhs = Min(lhs);
            rhs = Min(rhs);
            return new Cell(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
        }

        public static Cell Max(Cell lhs, Cell rhs)
        {
            lhs = Max(lhs);
            rhs = Max(rhs);
            return new Cell(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
        }

        public Cell ConvertTo(CellCoordinateSystem system) => new Cell(system, _x, _y, _edge);

        /// <summary>
        /// Returns true if the cell is equal to the given cell
        /// </summary>
        /// <param name="other">Cell to compare to</param>
        /// <returns>True if the two cells are equal</returns>
        public bool Equals(Cell other) => _system == other._system && _edge == other._edge && _x == other._x && _y == other._y;

        public static bool operator == (Cell lhs, Cell rhs) => lhs.Equals(rhs);

        public static bool operator != (Cell lhs, Cell rhs) => !lhs.Equals(rhs);

        public static Cell operator + (Cell lhs, Vector2Int rhs) => new Cell(lhs.system, lhs.x + rhs.x, lhs.y + rhs.y, lhs.edge);
        public static Cell operator - (Cell lhs, Vector2Int rhs) => new Cell(lhs.system, lhs.x - rhs.x, lhs.y - rhs.y, lhs.edge);

        public static Vector2Int operator - (Cell lhs, Cell rhs) => new Vector2Int(lhs.x - rhs.x, lhs.y - rhs.y);

        public override bool Equals(object other) => other.GetType() == typeof(Cell) && Equals((Cell)other);

        public override int GetHashCode() => (_x & 0x7FF) + ((_y & 0x7FF) << 11) + ((int)_edge << 22) + ((int)_system << 28);

        public bool IsAdjacentTo(Cell other) => DistanceTo(other) == 1;

        public int DistanceTo(Cell other) => Mathf.Max(Mathf.Abs(x - other.x), Mathf.Abs(y - other.y));

        public Vector2 ToVector2() => new Vector2(x, y);

        public Vector3 ToVector3() => new Vector3(x, 0, y);

        public Vector3Int ToVector3Int() => new Vector3Int(x, 0, y);

        public static Cell GetParallelEdge(Cell cell, CellEdgeSide cornerSide)
        {
            switch (cell.system)
            {
                case CellCoordinateSystem.SharedEdge:
                    switch (cell.edge)
                    {
                        case CellEdge.North:
                            return cell + (cornerSide == CellEdgeSide.Min ? Vector2Int.left : Vector2Int.right);

                        case CellEdge.East:
                            return cell + (cornerSide == CellEdgeSide.Min ? Vector2Int.up : Vector2Int.down);

                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }

            return invalid;
        }


        public static Cell PerpendicularEdge (Cell cell, CellEdgeSide corerSide, CellEdgeSide perpendicularSide)
        {
            switch (cell.system)
            {
                case CellCoordinateSystem.SharedEdge:
                    switch (cell.edge)
                    {
                        case CellEdge.North:
                            return new Cell(
                                cell +
                                (corerSide == CellEdgeSide.Min ? 1 : 0) * Vector2Int.left +
                                (perpendicularSide == CellEdgeSide.Max ? 1 : 0) * Vector2Int.up,
                                CellEdge.East);

                        case CellEdge.East:
                            return new Cell(
                                cell +
                                (corerSide == CellEdgeSide.Min ? 1 : 0) * Vector2Int.down +
                                (perpendicularSide == CellEdgeSide.Max ? 1 : 0) * Vector2Int.right,
                                CellEdge.North);

                        default:
                            break;                            
                    }
                    break;

                default:
                    break;
            }

            return invalid;
        }

        public override string ToString()
        {
            switch (_system)
            {
                default:
                    throw new NotImplementedException();

                case CellCoordinateSystem.Grid:
                    return $"({_x},{_y})";

                case CellCoordinateSystem.SharedEdge:
                case CellCoordinateSystem.Edge:
                    return $"({_x},{_y},{_edge})";
            }
        }

        public static Vector2Int EdgeToDirection(CellEdge edge)
        {
            switch (edge)
            {
                case CellEdge.East:
                    return new Vector2Int(1, 0);
                case CellEdge.West:
                    return new Vector2Int(-1, 0);
                case CellEdge.North:
                    return new Vector2Int(0, 1);
                case CellEdge.South:
                    return new Vector2Int(0, -1);
                default:
                    return Vector2Int.zero;
            }
        }

        public static CellEdge DirectionToEdge(Vector2Int dir)
        {
            if (dir.x != 0 && dir.y != 0)
                return CellEdge.None;

            if (dir.x > 0)
                return CellEdge.East;
            else if (dir.x < 0)
                return CellEdge.West;
            else if (dir.y < 0)
                return CellEdge.South;
            else if (dir.y > 0)
                return CellEdge.North;

            return CellEdge.None;
        }

        public static bool IsOppositeEdge(CellEdge lhs, CellEdge rhs)
        {
            return
                (lhs == CellEdge.East && rhs == CellEdge.West) ||
                (lhs == CellEdge.West && rhs == CellEdge.East) ||
                (lhs == CellEdge.North && rhs == CellEdge.South) ||
                (lhs == CellEdge.South && rhs == CellEdge.North);
        }
    }

    public struct CellBounds
    {
        public static readonly CellBounds invalid = new CellBounds(Cell.invalid, Cell.invalid);

        public Cell min;
        public Cell max;

        public CellBounds(Cell min, Cell max)
        {
            this.min = Cell.Min(min, max);
            this.max = Cell.Max(min, max);
        }

        public bool Contains(Cell cell)
        {
            // Account for shared edges
            var cmin = Cell.Min(cell);
            var cmax = Cell.Max(cell);

            // Cell within the bounds?
            return cmin.x >= min.x && cmax.x <= max.x && cmin.y >= min.y && cmax.y <= max.y;
        }
    }
}
