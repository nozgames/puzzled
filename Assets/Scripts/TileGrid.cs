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

        private static TileGrid _instance = null;

        /// <summary>
        /// Returns the size of the tile grid
        /// </summary>
        public static int size => _instance._size;

        /// <summary>
        /// Returns true if the tile grid is within the LinkTile call
        /// </summary>
        internal static bool isLinking => _instance._linking;

        private Tile[] _tiles;
        private int _layerCount;
        private int _stride;
        private int _center;
        private bool _linking;

        private void OnEnable()
        {
            _instance = this;

            _layerCount = Enum.GetNames(typeof(TileLayer)).Length;
            _tiles = new Tile[_size * _size * _layerCount];
            _stride = _layerCount * _size;
            _center = (_size / 2) * _stride + (_size / 2) * _layerCount;
        }

        private void OnDisable()
        {
            _instance = null;
        }

        /// <summary>
        /// Generates an array of all currently linked tiles.  Note that this method allocates
        /// an entirely new list and is just a snapshop of the linked tiles at the time of the call.
        /// </summary>
        /// <returns></returns>
        public static Tile[] GetLinkedTiles() => _instance._tiles.Where(t => t != null).ToArray();

        /// <summary>
        /// Return the first tile that matches the given tile info
        /// </summary>
        /// <param name="tileInfo">Tile info to search for</param>
        /// <returns>Tile that matches the given tile info or null if none found</returns>
        public static Tile GetLinkedTile(TileInfo tileInfo) => _instance._tiles.Where(t => t != null && t.info == tileInfo).FirstOrDefault();

        /// <summary>
        /// Convert a world coordinate to a cell coordinate
        /// </summary>
        /// <param name="position">World coordinate</param>
        /// <returns>Cell coorindate</returns>
        public static Cell WorldToCell(Vector3 position) => _instance._grid.WorldToCell(position).ToCell();

        /// <summary>
        /// Convert a cell coordinate to a world coordinate
        /// </summary>
        /// <param name="cell">Cell Coordiate</param>
        /// <returns>World Coordinate</returns>
        public static Vector3 CellToWorld(Cell cell) => _instance._grid.CellToWorld(cell.ToVector3Int());

        /// <summary>
        /// Convert the given cell to a tile index
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <returns>Index into tile array</returns>
        private static int CellToIndex(Cell cell) => 
            _instance._center + cell.x * _instance._layerCount + cell.y * _instance._stride;

        /// <summary>
        /// Convert the given cell and layer to a tile index
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>Index into the tile array</returns>
        private static int CellToIndex(Cell cell, TileLayer layer) => CellToIndex(cell) + (int)layer;

        /// <summary>
        /// Convert the given cell and layer to a tile
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>Tile at the given cell and layer or null if none</returns>
        public static Tile CellToTile(Cell cell, TileLayer layer) => _instance._tiles[CellToIndex(cell, layer)];

        /// <summary>
        /// Returns the top most tile in the given cell
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <returns>Topmost tile in the cell</returns>
        public static Tile CellToTile(Cell cell)
        {
            var tiles = _instance._tiles;
            var index = CellToIndex(cell);
            for (int layerIndex = _instance._layerCount - 1; layerIndex >= 0; layerIndex--)
                if (tiles[index + layerIndex] != null)
                    return tiles[index + layerIndex];

            return null;
        }

        /// <summary>
        /// Link the given tile into the tile grid using its current cell and layer
        /// </summary>
        /// <param name="tile"></param>
        public static void LinkTile (Tile tile)
        {
            Debug.Assert(tile != null);

            var tiles = _instance._tiles;
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

            _instance._linking = true;

            tiles[index] = tile;

            // Ensure the tile is parented to the grid
            if (tile.transform.parent != _instance._grid.transform)
                tile.transform.SetParent(_instance._grid.transform);

            // Move the tile to the correct world position
            tile.transform.position = CellToWorld(tile.cell);

            _instance._linking = false;
        }

        /// <summary>
        /// Unlink the tile at the given index
        /// </summary>
        /// <param name="index">Tile index</param>
        /// <param name="destroy">True if the tile should be destroyed after being unlinked</param>
        private static void UnlinkTile(int index, bool destroy)
        {
            var tiles = _instance._tiles;
            if (index < 0 || index > tiles.Length)
                return;

            _instance._linking = true;

            if (destroy && tiles[index] != null)
                tiles[index].Destroy();

            tiles[index] = null;

            _instance._linking = false;
        }

        /// <summary>
        /// Unlink the tile at the given cell and layer
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <param name="destroy">True if the tile should be destroyed after being unlinked</param>
        public static void UnlinkTile(Cell cell, TileLayer layer, bool destroy = false) =>
            UnlinkTile(CellToIndex(cell, layer), destroy);

        /// <summary>
        /// Unlink the given tile from the tile grid
        /// </summary>
        /// <param name="tile">Tile to unlink</param>
        /// <param name="destroy">True if the tile should be destroyed after it is unlinked</param>
        public static void UnlinkTile(Tile tile, bool destroy = false)
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
        public static void UnlinkTiles (Cell cell, bool destroy = false)
        {
            var index = CellToIndex(cell);
            for (int i = _instance._layerCount - 1; i >= 0; i--)
                UnlinkTile(index + i, destroy);
        }

        /// <summary>
        /// Unlink all tiles in the grid
        /// <param name="destroy">True if the tile should be destroyed when unlinking</param>
        /// </summary>
        public static void UnlinkAll (bool destroy = false)
        {
            var tiles = _instance._tiles;
            for (int i = 0; i < tiles.Length; i++)
                UnlinkTile(i, destroy);
        }

        /// <summary>
        /// Returns true if the given layer within the given cell is linked to a tile.
        /// </summary>
        /// <param name="cell">Cell coordinate</param>
        /// <param name="layer">Tile layer</param>
        /// <returns>True if the layer for the given cell is linked to a tile.</returns>
        public static bool IsLinked(Cell cell, TileLayer layer) => CellToTile(cell, layer) != null;

        /// <summary>
        /// Send an event to a given cell
        /// </summary>
        public static bool SendToCell(ActorEvent evt, Cell cell, CellEventRouting routing = CellEventRouting.All)
        {
            var tiles = _instance._tiles;
            var index = CellToIndex(cell);

            if (index < 0 || index > _instance._tiles.Length)
                return false;

            switch (routing)
            {
                case CellEventRouting.All:
                {
                    var handled = false;
                    for (int layer = _instance._layerCount - 1; layer >= 0; layer--)
                    {
                        var tile = tiles[index + layer];
                        if (null == tile)
                            continue;

                        tile.Send(evt);
                        handled |= evt.IsHandled;
                    }

                    return handled;
                }

                case CellEventRouting.FirstHandled:
                {
                    for (int layer = _instance._layerCount- 1; layer >= 0 && !evt.IsHandled; layer--)
                    {
                        var tile = tiles[index + layer];
                        if (null == tile)
                            continue;

                        tile.Send(evt);
                    }
                    break;
                }

                case CellEventRouting.FirstVisible:
                {
                    for (int layer = _instance._layerCount - 1; layer >= 0; layer--)
                    {
                        var tile = tiles[index + layer];
                        if (null == tile)
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
