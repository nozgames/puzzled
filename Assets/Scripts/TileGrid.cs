using System;
using System.Linq;
using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class TileGrid : MonoBehaviour
    {
        private const float CellEdgeHintBias = 0.25f;
        private const float CellEdgeSize = 0.25f;

        [SerializeField] private int _size = 255;
        [SerializeField] private Grid _grid = null;

        /// <summary>
        /// Returns the size of the tile grid
        /// </summary>
        public int size => _size;

        public Cell minCell => new Cell(-_size / 2, -_size / 2);
        public Cell maxCell => new Cell(_size / 2, _size / 2);

        private Tile[][] _tiles;
        private int _layerCount;
        private int _layerStride;
        private int _stride;
        private int _center;

        /// <summary>
        /// Returns true if the given layer is an edge layer
        /// </summary>
        /// <param name="layer">Layer</param>
        /// <returns>True if the layer is an edge layer</returns>
        public static bool IsEdgeLayer(TileLayer layer) => layer == TileLayer.Wall || layer == TileLayer.WallStatic;

        private void Awake ()
        {
            _layerCount = Enum.GetNames(typeof(TileLayer)).Length;
            _tiles = new Tile[_layerCount][];
            _stride = _size;
            _layerStride = _size * size;
            _center = (_size / 2) * _stride + (_size / 2) * _layerCount;

            // Allocate all layers
            for (int i = 0; i < _layerCount; i++)
                _tiles[i] = new Tile[_layerStride * (IsEdgeLayer((TileLayer)i) ? 2 : 1)];
        }

        /// <summary>
        /// Generates an array of all currently linked tiles.  Note that this method allocates
        /// an entirely new list and is just a snapshop of the linked tiles at the time of the call.
        /// </summary>
        /// <returns></returns>
        public Tile[] GetLinkedTiles() => _tiles.SelectMany(t => t.Where(t => t != null)).ToArray();

        /// <summary>
        /// Get all linked tiles within the given bounds
        /// </summary>
        /// <param name="min">Min cell</param>
        /// <param name="max">Max cdell</param>
        /// <returns>Array of tiles</returns>
        public Tile[] GetLinkedTiles(Cell min, Cell max) => GetLinkedTiles(new CellBounds(min, max));

        /// <summary>
        /// Get all linked tiles within the given bounds
        /// </summary>
        /// <param name="bounds">Cell bounds</param>
        /// <returns>Array of tiles</returns>
        public Tile[] GetLinkedTiles(CellBounds bounds) =>
            _tiles.SelectMany(l => l.Where(t => t != null && bounds.Contains(t.cell))).ToArray();

        /// <summary>
        /// Get all linked tiles for the given cell
        /// </summary>
        /// <param name="Cell">cell</param>
        /// <returns>Array of tiles</returns>
        public Tile[] GetLinkedTiles(Cell cell) => _tiles.SelectMany(l => l.Where(t => t != null && t.cell == cell)).ToArray();

        /// <summary>
        /// Return the first tile that matches the given tile info
        /// </summary>
        /// <param name="tileInfo">Tile info to search for</param>
        /// <returns>Tile that matches the given tile info or null if none found</returns>
        public Tile GetLinkedTile(Guid guid) => _tiles.Select(l => l.Where(t => t != null && t.guid == guid).FirstOrDefault()).FirstOrDefault();

        private Tile[] GetTiles(TileLayer layer) => _tiles[(int)layer];

        /// <summary>
        /// Convert a world coordinate to a cell coordinate
        /// </summary>
        /// <param name="position">World coordinate</param>
        /// <returns>Cell coorindate</returns>
        public Cell WorldToCell(Vector3 position)
        {
            var cellPosition = _grid.WorldToCell(position);
            var offset = position - (_grid.CellToWorld(cellPosition) + new Vector3(0.5f, 0, 0.5f));
            var absoffset = new Vector3(Mathf.Abs(offset.x), 0, Mathf.Abs(offset.z));
            var edge = Cell.Edge.None;

            if (absoffset.x > absoffset.z)
                edge = (offset.x > 0f) ? Cell.Edge.East : Cell.Edge.West;
            else
                edge = (offset.z > 0f) ? Cell.Edge.North : Cell.Edge.South;

            return new Cell(cellPosition.x, cellPosition.y, edge);
        }

        /// <summary>
        /// Convert a cell coordinate to a world coordinate
        /// </summary>
        /// <param name="cell">Cell Coordiate</param>
        /// <returns>World Coordinate</returns>
        public Vector3 CellToWorld(Cell cell) => _grid.CellToWorld(cell.ToVector3Int());

        private int CellToIndex(Cell cell) => _center + cell.x + cell.y * _stride;

        /// <summary>
        /// Convert the given cell and layer to a tile index
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>Index into the tile array</returns>
        private int CellToIndex(Cell cell, TileLayer layer)
        {
            var index = CellToIndex(cell);

            if (IsEdgeLayer(layer))
            {
                int offset = 0;

                switch (cell.edge)
                {
                    default:
                        return -1;

                    case Cell.Edge.West:
                        index--;
                        offset = 1;
                        break;

                    case Cell.Edge.East:                        
                        offset = 1;
                        break;

                    case Cell.Edge.North:
                        break;

                    case Cell.Edge.South:
                        index -= _stride;
                        break;
                }

                index = index * 2 + offset;
            }

            return index;
        }

        /// <summary>
        /// Convert the given cell and layer to a tile
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>Tile at the given cell and layer or null if none</returns>
        public Tile CellToTile(Cell cell, TileLayer layer)
        {
            if (cell.isEdge != IsEdgeLayer(layer))
                return null;

            var tiles = GetTiles(layer);
            var index = CellToIndex(cell, layer);
            if (index < 0 || index >= tiles.Length)
                return null;
            
            return tiles[index];
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
            var edgeLayer = (cell.edge != Cell.Edge.None);

            // Normal cells
            for (var layer = TileLayer.Logic; layer >= TileLayer.Floor; layer--)
                if (edgeLayer == IsEdgeLayer(layer))
                {
                    var tiles = GetTiles(layer);
                    var index = CellToIndex(cell, layer);
                    if (tiles[index] != null)
                        return tiles[index];
                }

            return null;
        }

        /// <summary>
        /// Link the given tile into the tile grid using its current cell and layer
        /// </summary>
        /// <param name="tile"></param>
        public void LinkTile (Tile tile)
        {
            if (null == tile)
                return;

            var tiles = _tiles[(int)tile.layer];
            var index = CellToIndex(tile.cell, tile.layer);
            if (index <= 0 || index >= tiles.Length)
                return;

            if(tiles[index] == tile)
            {
                Debug.LogWarning($"Tile '{tile.info.displayName}` in cell `{tile.cell}' was already linked.");
                return;
            }

            // Do not allow a tile to be linked to a cell that is occupied
            if(tiles[index] != null)
            {
                Debug.LogError($"Failed to link tile '{tile.info.displayName}` to cell `{tile.cell}', cell occupied by `{tiles[index].info.displayName}");
                return;
            }

            tiles[index] = tile;
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

            var tiles = _tiles[(int)tile.layer];
            var index = CellToIndex(tile.cell, tile.layer);
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
        /// Returns the world bounds for a given cell
        /// </summary>
        /// <param name="cell">Cell to return the bounds for</param>
        /// <returns>Cell bounds</returns>
        public Bounds CellToWorldBounds(Cell cell)
        {
            var world = CellToWorld(cell);
            switch (cell.edge)
            {
                default:
                    return new Bounds(world, new Vector3(1, 0, 1));

                case Cell.Edge.North:
                    return new Bounds(world + new Vector3(0, 0, 0.5f), new Vector3(1, 0, CellEdgeSize));

                case Cell.Edge.South:
                    return new Bounds(world - new Vector3(0, 0, 0.5f), new Vector3(1, 0, CellEdgeSize));

                case Cell.Edge.East:
                    return new Bounds(world + new Vector3(0.5f, 0, 0), new Vector3(CellEdgeSize, 0, 1));

                case Cell.Edge.West:
                    return new Bounds(world - new Vector3(0.5f, 0, 0), new Vector3(CellEdgeSize, 0, 1));
            }
        }

        /// <summary>
        /// Send an event to a given cell
        /// </summary>
        public bool SendToCell(ActorEvent evt, Cell cell, CellEventRouting routing = CellEventRouting.All)
        {
            // Validate the cell coordinates against the known grid
            var index = CellToIndex(cell);
            if (index < 0 || index >= _layerStride)
                return false;

            var edge = cell.edge != Cell.Edge.None;

            switch (routing)
            {
                case CellEventRouting.All:
                {
                    var handled = false;
                    for (var layer = TileLayer.Logic; layer >= TileLayer.Floor; layer--)
                    {
                        if (IsEdgeLayer(layer) != edge)
                            continue;

                        var tile = CellToTile(cell, layer);
                        if (null == tile || tile.isDestroyed)
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
                        if (IsEdgeLayer(layer) != edge)
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
                        if (IsEdgeLayer(layer) != edge)
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
