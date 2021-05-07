using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Wall : TileComponent
    {
        [SerializeField] private Transform _rotator = null;
        [SerializeField] private GameObject _cornerPrefab = null;
        [SerializeField] private GameObject _separatorPrefab = null;

        [SerializeField] private int _cornerPriority = 0;
        [SerializeField] private bool _smartCorners = true;

        [SerializeField] private bool _allowWallMounts = true;
        [SerializeField] private float _thickness = 0.1f;

        public float thickness => _thickness;

        /// <summary>
        /// True if the wall allows tils to be attached to it
        /// </summary>
        public bool allowsWallMounts => _allowWallMounts;

        private GameObject _cornerMin = null;
        private GameObject _cornerMax = null;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateVisuals(tile.cell, isEditing);
            UpdateMounts();
        }

        [ActorEventHandler]
        private void CellChangedEvent(CellChangedEvent evt)
        {
            if (!isEditing)
                return;

            UpdateVisuals(evt.old, true);
            UpdateVisuals(tile.cell, true);
            UpdateMounts();
        }

        [ActorEventHandler]
        private void OnQueryMoveEvent (QueryMoveEvent evt)
        {
            var edge = tile.cell.edge;
            if (evt.offset.x > 0 && edge == CellEdge.East)
                evt.result = false;
            else if (evt.offset.x < 0 && edge == CellEdge.East)
                evt.result = false;
            else if (evt.offset.y < 0 && edge == CellEdge.North)
                evt.result = false;
            else if (evt.offset.y > 0 && edge == CellEdge.North)
                evt.result = false;
        }

        private int ComparePriority (Wall compare)
        {
            if (compare == null)
                return 1;

            if (_cornerPrefab == null)
                return -1;

            if (_cornerPriority > compare._cornerPriority)
                return 1;

            if (_cornerPriority < compare._cornerPriority)
                return -1;

            // Priority is the same so use the edge to choose one
            var cell = tile.cell;
            var compareCell = compare.tile.cell;

            if (cell.edge > compareCell.edge)
                return 1;

            if (cell.edge < compareCell.edge)
                return -1;

            if (cell.x > compareCell.x)
                return 1;
            else if (compareCell.x > cell.x)
                return -1;

            return cell.y - compareCell.y;
        }

        /// <summary>
        /// Return all walls in the N/E corner of the given cell
        /// </summary>
        /// <param name="cell">Cell to return the walls from</param>
        /// <returns>Array of walls in the N/E corner of the cell </returns>
        private Wall[] GetCornerWalls (Cell cell)
        {
            var list = new List<Wall>();
            list.Add(puzzle.grid.CellToComponent<Wall>(new Cell(cell, CellEdge.North), TileLayer.Wall));
            list.Add(puzzle.grid.CellToComponent<Wall>(new Cell(cell, CellEdge.East), TileLayer.Wall));
            list.Add(puzzle.grid.CellToComponent<Wall>(new Cell(cell + Vector2Int.right, CellEdge.North), TileLayer.Wall));
            list.Add(puzzle.grid.CellToComponent<Wall>(new Cell(cell + Vector2Int.up, CellEdge.East), TileLayer.Wall));
            return list.Where(w => w != null).ToArray();
        }

        private GameObject AddCorner (Cell cell, Vector3 offset, bool updateNeighbors)
        {
            var cornerWalls = GetCornerWalls(cell).Where(w => w != this).ToArray();

            // Check if any wall is a higher priority
            var ownsCorner = _cornerPrefab != null;

            // Smart corners means no corner when the wall is continuing in the same direction.
            var continuedEdge = cornerWalls.Length == 1 && cornerWalls[0].tile.cell.edge == tile.cell.edge && (cornerWalls[0].tile.guid == tile.guid || cornerWalls[0]._cornerPrefab == _cornerPrefab);
            if (_smartCorners && continuedEdge)
                ownsCorner = false;

            if(ownsCorner)
                foreach (var cornerWall in cornerWalls)
                {
                    var compare = ComparePriority(cornerWall);
                    if (compare < 0)
                    {
                        ownsCorner = false;
                        break;
                    }
                }

            // Create the corner if needed
            var corner = ownsCorner ? Instantiate(_cornerPrefab, _rotator) : 
                ((continuedEdge && _separatorPrefab != null) ? Instantiate(_separatorPrefab, _rotator) : null);

            if (corner != null)
                corner.transform.localPosition = offset;

            // Update all neighbors
            if (updateNeighbors)
                foreach (var cornerWall in cornerWalls)
                    cornerWall.UpdateVisuals(cornerWall.tile.cell, false);

            return corner;
        }

        private void UpdateVisuals(Cell cell, bool updateNeighbors = false)
        {
            // Destroy old corners
            if (_cornerMin != null)
            {
                Destroy(_cornerMin);
                _cornerMin = null;
            }

            if (_cornerMax != null)
            {
                Destroy(_cornerMax);
                _cornerMax = null;
            }

            if (cell == Cell.invalid)
                return;

            // Rotate wall when on the east edge
            if (cell.edge == CellEdge.East)
            {
                _rotator.transform.localEulerAngles = new Vector3(0, 90, 0);

                _cornerMin = AddCorner(cell, new Vector3(-0.5f, 0), updateNeighbors);
                _cornerMax = AddCorner(cell - Vector2Int.up, new Vector3(0.5f, 0), updateNeighbors);
            }
            else
            {
                _cornerMin = AddCorner(cell, new Vector3(0.5f, 0), updateNeighbors);
                _cornerMax = AddCorner(cell - Vector2Int.right, new Vector3(-0.5f, 0), updateNeighbors);
            }
        }

        private WallMounted GetMounted (CellEdgeSide side)
        {
            return side == CellEdgeSide.Min ?
                (tile.grid.CellToComponent<WallMounted>(tile.cell.ConvertTo(CellCoordinateSystem.Edge), TileLayer.WallStatic)) :
                (tile.grid.CellToComponent<WallMounted>(
                    tile.cell.edge == CellEdge.East ?
                        new Cell(CellCoordinateSystem.Edge, tile.cell + new Vector2Int(1, 0), CellEdge.West) :
                        new Cell(CellCoordinateSystem.Edge, tile.cell + new Vector2Int(0, 1), CellEdge.South),
                    TileLayer.WallStatic));
        }

        public Tile[] GetMountedTiles ()
        {
            var mounted0 = GetMounted(CellEdgeSide.Min);
            var mounted1 = GetMounted(CellEdgeSide.Max);

            if (mounted0 != null && mounted1 != null)
                return new Tile[] { mounted0.tile, mounted1.tile };
            else if (mounted0 != null)
                return new Tile[] { mounted0.tile };
            else if (mounted1 != null)
                return new Tile[] { mounted1.tile };

            return new Tile[] { };
        }

        private void UpdateMounts()
        {
            if (tile.cell == Cell.invalid)
                return;

            var mountedMin = GetMounted(CellEdgeSide.Min);
            if (mountedMin != null)
                mountedMin.UpdateVisuals();

            var mountedMax = GetMounted(CellEdgeSide.Max);
            if (mountedMax != null)
                mountedMax.UpdateVisuals();
        }
    }
}
