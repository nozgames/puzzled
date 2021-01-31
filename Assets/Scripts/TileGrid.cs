using System;
using System.Linq;
using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class TileGrid : MonoBehaviour
    {
        [SerializeField] private int _size = 255;
        [SerializeField] private Grid _grid = null;

        /// <summary>
        /// Returns the size of the tile grid
        /// </summary>
        public int size => _size;

        /// <summary>
        /// Returns true if the tile grid is within the LinkTile call
        /// </summary>
        internal bool isLinking => _linking;

        public Cell minCell => new Cell(-_size / 2, -_size / 2);
        public Cell maxCell => new Cell(_size / 2, _size / 2);

        private Tile[] _tiles;
        private int _layerCount;
        private int _stride;
        private int _center;
        private bool _linking;

        private void Awake ()
        {
            _layerCount = Enum.GetNames(typeof(TileLayer)).Length;
            _tiles = new Tile[_size * _size * _layerCount];
            _stride = _layerCount * _size;
            _center = (_size / 2) * _stride + (_size / 2) * _layerCount;
        }

        /// <summary>
        /// Generates an array of all currently linked tiles.  Note that this method allocates
        /// an entirely new list and is just a snapshop of the linked tiles at the time of the call.
        /// </summary>
        /// <returns></returns>
        public Tile[] GetLinkedTiles() => _tiles.Where(t => t != null).ToArray();

        /// <summary>
        /// Get all linked tiles within the given bounds
        /// </summary>
        /// <param name="min">Min cell</param>
        /// <param name="max">Max cdell</param>
        /// <returns>Array of tiles</returns>
        public Tile[] GetLinkedTiles(Cell min, Cell max) => _tiles.Where(t => t != null && t.cell.x >= min.x && t.cell.x <= max.x && t.cell.y >= min.y && t.cell.y <= max.y).ToArray();

        /// <summary>
        /// Return the first tile that matches the given tile info
        /// </summary>
        /// <param name="tileInfo">Tile info to search for</param>
        /// <returns>Tile that matches the given tile info or null if none found</returns>
        public Tile GetLinkedTile(TileInfo tileInfo) => _tiles.Where(t => t != null && t.info == tileInfo).FirstOrDefault();

        /// <summary>
        /// Convert a world coordinate to a cell coordinate
        /// </summary>
        /// <param name="position">World coordinate</param>
        /// <returns>Cell coorindate</returns>
        public Cell WorldToCell(Vector3 position) => _grid.WorldToCell(position).ToCell();

        /// <summary>
        /// Convert a cell coordinate to a world coordinate
        /// </summary>
        /// <param name="cell">Cell Coordiate</param>
        /// <returns>World Coordinate</returns>
        public Vector3 CellToWorld(Cell cell) => _grid.CellToWorld(cell.ToVector3Int());

        /// <summary>
        /// Convert the given cell to a tile index
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <returns>Index into tile array</returns>
        private int CellToIndex(Cell cell) => 
            _center + cell.x * _layerCount + cell.y * _stride;

        /// <summary>
        /// Convert the given cell and layer to a tile index
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>Index into the tile array</returns>
        private int CellToIndex(Cell cell, TileLayer layer) => CellToIndex(cell) + (int)layer;

        /// <summary>
        /// Convert the given cell and layer to a tile
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>Tile at the given cell and layer or null if none</returns>
        public Tile CellToTile(Cell cell, TileLayer layer)
        {
            var index = CellToIndex(cell, layer);
            if (index < 0 || index >= _tiles.Length)
                return null;
            
            return _tiles[index];
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
            var tiles = _tiles;
            var index = CellToIndex(cell);
            for (int layerIndex = _layerCount - 1; layerIndex >= 0; layerIndex--)
                if (tiles[index + layerIndex] != null)
                    return tiles[index + layerIndex];

            return null;
        }

        /// <summary>
        /// Link the given tile into the tile grid using its current cell and layer
        /// </summary>
        /// <param name="tile"></param>
        public void LinkTile (Tile tile)
        {
            Debug.Assert(tile != null);

            var tiles = _tiles;
            var index = CellToIndex(tile.cell, tile.info.layer);
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

            _linking = true;

            tiles[index] = tile;

            // Ensure the tile is parented to the grid
            if (tile.transform.parent != _grid.transform)
                tile.transform.SetParent(_grid.transform);

            // Move the tile to the correct world position
            tile.transform.position = CellToWorld(tile.cell);

            _linking = false;
        }

        /// <summary>
        /// Unlink the tile at the given index
        /// </summary>
        /// <param name="index">Tile index</param>
        /// <param name="destroy">True if the tile should be destroyed after being unlinked</param>
        private void UnlinkTile(int index, bool destroy)
        {
            var tiles = _tiles;
            if (index < 0 || index > tiles.Length)
                return;

            _linking = true;

            var tile = tiles[index];
            tiles[index] = null;
            if (destroy && tile != null)
                tile.Destroy();
            else
                tile.cell = Cell.invalid;

            _linking = false;
        }

        /// <summary>
        /// Unlink the tile at the given cell and layer
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <param name="destroy">True if the tile should be destroyed after being unlinked</param>
        public void UnlinkTile(Cell cell, TileLayer layer, bool destroy = false) =>
            UnlinkTile(CellToIndex(cell, layer), destroy);

        /// <summary>
        /// Unlink the given tile from the tile grid
        /// </summary>
        /// <param name="tile">Tile to unlink</param>
        /// <param name="destroy">True if the tile should be destroyed after it is unlinked</param>
        public void UnlinkTile(Tile tile, bool destroy = false)
        {
            if (null == tile)
                return;

            // Ensure the tile being unlinked is the tile that is linked
            if (CellToTile(tile.cell, tile.info.layer) != tile)
                return;

            UnlinkTile(tile.cell, tile.info.layer, destroy);
        }

        /// <summary>
        /// Unlink all tiles at the given cell
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="destroy">True if the tile should be destroyed when unlinking</param>
        public void UnlinkTiles (Cell cell, bool destroy = false)
        {
            var index = CellToIndex(cell);
            for (int i = _layerCount - 1; i >= 0; i--)
                UnlinkTile(index + i, destroy);
        }

        /// <summary>
        /// Unlink all tiles in the grid
        /// <param name="destroy">True if the tile should be destroyed when unlinking</param>
        /// </summary>
        public void UnlinkAll (bool destroy = false)
        {
            var tiles = _tiles;
            for (int i = 0; i < tiles.Length; i++)
                UnlinkTile(i, destroy);
        }

        /// <summary>
        /// Returns true if the given layer within the given cell is linked to a tile.
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>True if the layer for the given cell is linked to a tile.</returns>
        public bool IsLinked(Cell cell, TileLayer layer) => CellToTile(cell, layer) != null;

        /// <summary>
        /// Send an event to a given cell
        /// </summary>
        public bool SendToCell(ActorEvent evt, Cell cell, CellEventRouting routing = CellEventRouting.All)
        {
            var tiles = _tiles;
            var index = CellToIndex(cell);

            if (index < 0 || index > _tiles.Length)
                return false;

            switch (routing)
            {
                case CellEventRouting.All:
                {
                    var handled = false;
                    for (int layer = _layerCount - 1; layer >= 0; layer--)
                    {
                        var tile = tiles[index + layer];
                        if (null == tile || tile.isDestroyed)
                            continue;

                        tile.Send(evt);
                        handled |= evt.IsHandled;
                    }

                    return handled;
                }

                case CellEventRouting.FirstHandled:
                {
                    for (int layer = _layerCount- 1; layer >= 0 && !evt.IsHandled; layer--)
                    {
                        var tile = tiles[index + layer];
                        if (null == tile || tile.isDestroyed)
                            continue;

                        tile.Send(evt);
                    }
                    break;
                }

                case CellEventRouting.FirstVisible:
                {
                    for (int layer = (int)(TileLayer.Logic - 1); layer >= 0; layer--)
                    {
                        var tile = tiles[index + layer];
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
