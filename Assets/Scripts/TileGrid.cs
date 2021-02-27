using System;
using System.Linq;
using System.Reflection;
using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class TileGrid : MonoBehaviour
    {
        private const float CellEdgeSize = 0.35f;

        private struct Layer
        {
            public TileLayer id;
            public CellCoordinateSystem system;
            public int stride;
            public Tile[] tiles;
        }

        [SerializeField] private int _size = 255;
        [SerializeField] private Grid _grid = null;

        /// <summary>
        /// Returns the size of the tile grid
        /// </summary>
        public int size => _size;

        public Cell minCell => new Cell(-_size / 2, -_size / 2);
        public Cell maxCell => new Cell(_size / 2, _size / 2);

        private Layer[] _layers;

        private static readonly Vector3[][] _edgeWorldOffset = new Vector3[][] {
            // CoordinateSystem.Invalid
            new Vector3[] {
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero
            },
            // CoordinateSystem.Grid
            new Vector3[] {
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero
            },
            // CoordinateSystem.Edge
            new Vector3[] {
                Vector3.zero,
                new Vector3(0, 0, 0.5f - CellEdgeSize * 0.25f),    // North
                new Vector3(0.5f - CellEdgeSize * 0.25f,0,0),      // East
                new Vector3(0, 0, -0.5f + CellEdgeSize * 0.25f),   // South
                new Vector3(-0.5f + CellEdgeSize * 0.25f, 0, 0)    // West
            },
            // CoordinateSystem.SharedEdge
            new Vector3[] {
                Vector3.zero,
                new Vector3(0, 0, 0.5f),      // North
                new Vector3(0.5f, 0, 0),      // East
                new Vector3(0, 0, -0.5f),     // South
                new Vector3(-0.5f, 0, 0)      // West
            },
        };

        private static readonly Vector3[][] _edgeWorldSize = new Vector3[][] {
            // CoordinateSystem.Invalid
            new Vector3[] {
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero
            },
            // CoordinateSystem.Grid
            new Vector3[] {
                new Vector3(1, 0, 1),
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero
            },
            // CoordinateSystem.Edge
            new Vector3[] {
                Vector3.zero,
                new Vector3(1,0,CellEdgeSize*0.5f),  // North
                new Vector3(CellEdgeSize*0.5f,0,1),  // East
                new Vector3(1,0,CellEdgeSize*0.5f),  // South
                new Vector3(CellEdgeSize*0.5f,0,1)   // West
            },
            // CoordinateSystem.SharedEdge
            new Vector3[] {
                Vector3.zero,
                new Vector3(1,0,CellEdgeSize),      // North
                new Vector3(CellEdgeSize,0,1),      // East
                new Vector3(1,0,CellEdgeSize),      // South
                new Vector3(CellEdgeSize,0,1)       // West
            },
        };

        private int _center;

        private void Awake ()
        {
            _layers = Enum.GetNames(typeof(TileLayer)).Select(l => {
                var system = typeof(TileLayer).GetField(l).GetCustomAttribute<CoordinateSystemAttribute>()?.system ?? CellCoordinateSystem.Grid;
                var stride = 1;
                switch (system)
                {
                    case CellCoordinateSystem.Grid: stride = 1; break;
                    case CellCoordinateSystem.Edge: stride = 4; break;
                    case CellCoordinateSystem.SharedEdge: stride = 2; break;
                    default:
                        throw new NotImplementedException();
                }

                return new Layer {
                    id = Enum.TryParse<TileLayer>(l, out var id) ? id : TileLayer.Floor,
                    system = system,
                    stride = stride,
                    tiles = new Tile[stride * _size * _size]
                };
            }).ToArray();

            _center = (_size / 2) * (_size + 1);
        }

        /// <summary>
        /// Generates an array of all known tiles.  Note that this method allocates
        /// an entirely new list and is just a snapshop of the linked tiles at the time of the call.
        /// </summary>
        /// <returns></returns>
        public Tile[] GetTiles() => _layers.SelectMany(l => l.tiles.Where(t => t != null)).ToArray();

        /// <summary>
        /// Generate an array of all known tiles within the given bounds
        /// </summary>
        /// <param name="min">Min cell</param>
        /// <param name="max">Max cdell</param>
        /// <returns>Array of tiles</returns>
        public Tile[] GetTiles(Cell min, Cell max) => GetTiles(new CellBounds(min, max));

        /// <summary>
        /// Get all linked tiles within the given bounds
        /// </summary>
        /// <param name="bounds">Cell bounds</param>
        /// <returns>Array of tiles</returns>
        public Tile[] GetTiles(CellBounds bounds) =>
            _layers.SelectMany(l => l.tiles.Where(t => t != null && bounds.Contains(t.cell))).ToArray();

        /// <summary>
        /// Get all linked tiles for the given cell
        /// </summary>
        /// <param name="Cell">cell</param>
        /// <returns>Array of tiles</returns>
        public Tile[] GetTiles(Cell cell) => _layers.SelectMany(l => l.tiles.Where(t => t != null && t.cell == cell)).ToArray();

        /// <summary>
        /// Return the first tile that matches the given tile info
        /// </summary>
        /// <param name="tileInfo">Tile info to search for</param>
        /// <returns>Tile that matches the given tile info or null if none found</returns>
        public Tile CellToTile(Guid guid) => _layers.Select(l => l.tiles.Where(t => t != null && t.guid == guid).FirstOrDefault()).FirstOrDefault();

        /// <summary>
        /// Convert a world coordinate to a cell coordinate
        /// </summary>
        /// <param name="position">World coordinate</param>
        /// <returns>Cell coorindate</returns>
        public Cell WorldToCell(Vector3 position) => WorldToCell(position, CellCoordinateSystem.Grid);

        /// <summary>
        /// Convert a world coordinate to a cell coordinate
        /// </summary>
        /// <param name="position">World coordinate</param>
        /// <returns>Cell coorindate</returns>
        public Cell WorldToCell(Vector3 position, CellCoordinateSystem system)
        {
            if (system == CellCoordinateSystem.Invalid)
                return Cell.invalid;

            var cellPosition = _grid.WorldToCell(position);
            if (system == CellCoordinateSystem.Grid)
                return new Cell(cellPosition.x, cellPosition.y);

            var offset = position - (_grid.CellToWorld(cellPosition) + new Vector3(0.5f, 0, 0.5f));
            var absoffset = new Vector3(Mathf.Abs(offset.x), 0, Mathf.Abs(offset.z));
            var edge = CellEdge.None;

            if (absoffset.x > absoffset.z)
                edge = (offset.x > 0f) ? CellEdge.East : CellEdge.West;
            else
                edge = (offset.z > 0f) ? CellEdge.North : CellEdge.South;

            return new Cell(system, cellPosition.x, cellPosition.y, edge);
        }

        /// <summary>
        /// Convert the given coordinate and layer to an index within the layers tile array
        /// </summary>
        /// <param name="coordinate">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>Index into the tile array</returns>
        private int GetIndex(Cell coordinate)
        {
            switch (coordinate.system)
            {
                case CellCoordinateSystem.Grid:
                    return _center + coordinate.x + coordinate.y * size;

                case CellCoordinateSystem.SharedEdge:
                    return _center + coordinate.x * 2 + coordinate.y * size * 2 + (int)coordinate.edge;

                case CellCoordinateSystem.Edge:
                    return _center + coordinate.x * 4 + coordinate.y * size * 4 + (int)coordinate.edge;

                default:
                    throw new NotImplementedException();
            }
        }

        public CellCoordinateSystem LayerToCoordinateSystem (TileLayer layer) => _layers[(int)layer].system;

        /// <summary>
        /// Convert the given cell and layer to a tile
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>Tile at the given cell and layer or null if none</returns>
        public Tile CellToTile(Cell cell, TileLayer layer)
        {
            if (cell.system == CellCoordinateSystem.Invalid)
                return null;

            var layerInfo = _layers[(int)layer];
            if (layerInfo.system != cell.system)
                return null;

            var index = GetIndex(cell);
            if (index < 0 || index >= layerInfo.tiles.Length)
                return null;

            return layerInfo.tiles[index];
        }

        /// <summary>
        /// Return the component of th given type from the given cell and layer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cell"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public T CellToComponent<T> (Cell cell, TileLayer layer) where T : TileComponent
        {
            var tile = CellToTile(cell, layer);
            if (null == tile)
                return null;

            return tile.GetComponentInChildren<T>();
        }

        /// <summary>
        /// Returns the top most tile in the given cell
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <returns>Topmost tile in the cell</returns>
        public Tile CellToTile(Cell cell)
        {
            var index = GetIndex(cell);

            for (var layer = (int)TileLayer.Logic; layer >= 0; layer--)
                if(_layers[layer].system == cell.system)
                {
                    var tile = _layers[layer].tiles[index];
                    if (null != tile)
                        return tile;                   
                }

            return null;
        }

        public Cell LayerToCell(TileLayer layer) => new Cell(_layers[(int)layer].system);

        /// <summary>
        /// Link the given tile into the tile grid using its current cell and layer
        /// </summary>
        /// <param name="tile"></param>
        public bool LinkTile (Tile tile)
        {
            if (null == tile)
                return false;

            var layerInfo = _layers[(int)tile.layer];
            if (tile.cell.system != layerInfo.system)
                return false;

            var tiles = layerInfo.tiles;
            var index = GetIndex(tile.cell);
            if (index < 0 || index >= tiles.Length)
                return false;

            if(tiles[index] == tile)
            {
                Debug.LogWarning($"Tile '{tile.info.displayName}` in cell `{tile.cell}' was already linked.");
                return false;
            }

            // Do not allow a tile to be linked to a cell that is occupied
            if(tiles[index] != null)
            {
                Debug.LogError($"Failed to link tile '{tile.info.displayName}` to cell `{tile.cell}', cell occupied by `{tiles[index].info.displayName}");
                return false;
            }

            tiles[index] = tile;

            return true;
        }

        /// <summary>
        /// Unlink the given tile from the tile grid
        /// </summary>
        /// <param name="tile">Tile to unlink</param>
        public void UnlinkTile(Tile tile)
        {
            if (null == tile)
                return;

            // Ensure the tile being unlinked is the tile that is linked
            if (CellToTile(tile.cell, tile.layer) != tile)
                return;

            var tiles = _layers[(int)tile.layer].tiles;
            var index = GetIndex(tile.cell);
            if (index <= 0 || index >= tiles.Length)
                return;

            if (tiles[index] != tile)
                return;

            tiles[index] = null;
        }

        /// <summary>
        /// Returns true if the given layer within the given cell is linked to a tile.
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>True if the layer for the given cell is linked to a tile.</returns>
        public bool IsLinked(Cell cell, TileLayer layer) => CellToTile(cell, layer) != null;

        /// <summary>
        /// Convert a cell coordinate to a world coordinate
        /// </summary>
        /// <param name="cell">Cell Coordiate</param>
        /// <returns>World Coordinate</returns>
        public Vector3 CellToWorld(Cell cell) => _grid.CellToWorld(new Vector3Int(cell.x, cell.y, 0)) + _edgeWorldOffset[(int)cell.system][(int)cell.edge];

        /// <summary>
        /// Returns the world bounds for a given cell
        /// </summary>
        /// <param name="cell">Cell to return the bounds for</param>
        /// <returns>Cell bounds</returns>
        public Bounds CellToWorldBounds(Cell cell) => new Bounds(CellToWorld(cell),_edgeWorldSize[(int)cell.system][(int)cell.edge]);

        public bool CellContainsWorldPoint (Cell cell, Vector3 world)
        {
            var bounds = CellToWorldBounds(cell);

            return world.x >= bounds.min.x && world.x <= bounds.max.x &&
                   world.z >= bounds.min.z && world.z <= bounds.max.z;
        }

        /// <summary>
        /// Send an event to a given cell
        /// </summary>
        public bool SendToCell(ActorEvent evt, Cell cell, CellEventRouting routing = CellEventRouting.All)
        {
            switch (routing)
            {
                case CellEventRouting.All:
                {
                    var handled = false;
                    for (var layer = TileLayer.Logic; layer >= TileLayer.Floor; layer--)
                    {
                        if (cell.system != _layers[(int)layer].system)
                            continue;

                        var tile = CellToTile(cell, layer);
                        if (null == tile)
                            continue;

                        tile.Send(evt);
                        handled |= evt.IsHandled;
                    }

                    return handled;
                }

                case CellEventRouting.FirstHandled:
                {
                    for (var layer = TileLayer.Logic; layer >= TileLayer.Floor && !evt.IsHandled; layer--)
                    {
                        if (cell.system != _layers[(int)layer].system)
                            continue;

                        var tile = CellToTile(cell, layer);
                        if (null == tile || tile.isDestroyed)
                            continue;

                        tile.Send(evt);
                    }
                    break;
                }

                case CellEventRouting.FirstVisible:
                {
                    for (var layer = TileLayer.Logic - 1; layer >= TileLayer.Floor; layer--)
                    {
                        if (cell.system != _layers[(int)layer].system)
                            continue;

                        var tile = CellToTile(cell, layer);
                        if (null == tile || tile.isDestroyed)
                            continue;

                        tile.Send(evt);
                        return evt.IsHandled;
                    }
                    break;
                }
            }

            return evt.IsHandled;
        }
    }
}
