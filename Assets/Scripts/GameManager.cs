using NoZ;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Puzzled
{
    public class GameManager : ActorComponent
    {
        [SerializeField] private Grid grid = null;
        [SerializeField] private Canvas ui = null;
        [SerializeField] private int minimumPuzzleSize = 9;

        public InputActionReference menuAction;

        private Dictionary<Vector2Int, List<Tile>> cells = new Dictionary<Vector2Int, List<Tile>>();

        public static GameManager Instance { get; private set; }

        /// <summary>
        /// Returns the active puzzle
        /// </summary>
        public Puzzle puzzle { get; private set; }

        //public Puzzle TestPuzzle = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            menuAction.action.Enable();
            menuAction.action.performed += OnMenuAction;

            if (Instance == null)
                Instance = this;
        }

        protected override void OnDisable()
        {
            if (Instance == this)
                Instance = null;

            menuAction.action.Disable();
            menuAction.action.performed -= OnMenuAction;

            base.OnDisable();
        }

        private void OnMenuAction(InputAction.CallbackContext obj)
        {
            UIManager.instance.ShowIngame();
        }

        public static Vector2Int WorldToCell(Vector3 world) =>
            Instance.grid.WorldToCell(world).ToVector2Int();

        public static Vector3 CellToWorld(Vector2Int cell) =>
            Instance.grid.CellToWorld(cell.ToVector3Int());

        public static Tile[] GetTiles() => Instance.grid.GetComponentsInChildren<Tile>();

        private int busyCount = 0;

        public static bool IsBusy => Instance.busyCount > 0;

        public static void IncBusy() => Instance.busyCount++;

        public static void DecBusy() => Instance.busyCount--;


#if false
        public void LoadPuzzle (Puzzle puzzle)
        {
            //Screen.currentResolution
            //canvasScaler.referencePixelsPerUnit
            //canvasScaler.referenceResolution
            Camera.main.orthographicSize = /*ui.transform.localScale.x * */(Mathf.Max(minimumPuzzleSize, puzzle.Height) + 1) * 0.5f;

            for(int y=0; y<puzzle.Height; y++)
                Instantiate(puzzle.Theme.floor, grid.GetCellCenterWorld(new Vector3Int(0,-puzzle.Height/2 + y, 0)), Quaternion.identity, grid.transform);
        }
#endif

        public void LoadPuzzle(Puzzle puzzle) 
        {
            grid.transform.DetachAndDestroyChildren();

            busyCount = 0;

            this.puzzle = puzzle;

            Instantiate(puzzle.puzzlePrefab, grid.transform);

            LinkTiles();
        }

        /// <summary>
        /// Link all tiles that are children of the grid to a cell and fix their world position
        /// </summary>
        private void LinkTiles()
        {
            cells.Clear();

            var tiles = grid.GetComponentsInChildren<Tile>();
            foreach (var tile in tiles)
            {
                tile.cell = grid.WorldToCell(tile.transform.position).ToVector2Int();
                tile.transform.position = grid.CellToWorld(tile.cell.ToVector3Int());
            }
        }

        /// <summary>
        /// Return the cell for a given tile
        /// </summary>
        public Vector2Int GetTileCell(Tile tile) => grid.WorldToCell(tile.transform.position).ToVector2Int();

        /// <summary>
        /// Set the tiles current cell
        /// </summary>
        public void SetTileCell(Tile tile, Vector2Int cell)
        {
            if (tile.transform.parent != grid.transform)
                tile.transform.SetParent(grid.transform);

            // Remove the tile from the previous cell
            GetCellTiles(tile.cell)?.Remove(tile);

            // Add the tile to the give ncell
            var tiles = GetCellTiles(cell);
            if (null == tiles)
            {
                tiles = new List<Tile>(4) { tile };
                cells[cell] = tiles;
            }
            else
                tiles.Add(tile);

            tile.transform.position = grid.CellToWorld(cell.ToVector3Int());
        }

        /// <summary>
        /// Returns the list of tiles at the given cell
        /// </summary>
        public List<Tile> GetCellTiles(Vector2Int cell) =>
            cells.TryGetValue(cell, out var tiles) ? tiles : null;

        /// <summary>
        /// Send an event to a given cell
        /// </summary>
        public void SendToCell(ActorEvent evt, Vector2Int cell) 
        {
            var tiles = GetCellTiles(cell);
            if (null == tiles)
                return;

            for(var tileIndex=0; tileIndex<tiles.Count && !evt.IsHandled; tileIndex++)               
                tiles[tileIndex].Send(evt);
        }

        [ActorEventHandler]
        private void OnLevelExit(LevelExitEvent evt)
        {
            UIManager.instance.ShowPuzzleComplete();
        }

        public void ClearTiles ()
        {
            cells.Clear();
            grid.transform.DetachAndDestroyChildren();
        }

        public Tile InstantiateTile (Tile prefab, Vector2Int cell, int variantIndex=0)
        {
            var tile = Instantiate(prefab.gameObject, grid.transform).GetComponent<Tile>();
            tile.cell = cell;
            tile.transform.position = grid.CellToWorld(tile.cell.ToVector3Int());
            return tile;
        }

        public void ClearTile (Vector2Int cell, TileLayer layer)
        {
            var tiles = GetCellTiles(cell);
            if (null == tiles)
                return;

            foreach (var tile in tiles)
                if(tile.info.layer == layer)
                    Destroy(tile.gameObject);
        }

        public void ClearTile(Vector2Int cell)
        {
            var tiles = GetCellTiles(cell);
            if (null == tiles)
                return;

            foreach (var tile in tiles)
                Destroy(tile.gameObject);
        }

        public void RemoveTileFromCell (Tile tile)
        {
            var tiles = GetCellTiles(tile.cell);
            if (null == tiles)
                return;

            tiles.Remove(tile);
        }
    }
}
