using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        [Header("Selection")]
        [SerializeField] private Color _selectionColor = Color.red;

        public static Color selectionColor => instance._selectionColor;

        private List<Tile> _selectedTiles = new List<Tile>();
        private Wire _selectedWire = null;

        /// <summary>
        /// True if there is at least one tile selected
        /// </summary>
        public bool hasSelection => _selectedTiles.Count > 0;

        /// <summary>
        /// Creates a copy of the selected tiles array and returns it
        /// </summary>
        public static Tile[] selectedTiles {
            get => instance._selectedTiles.ToArray();
            set => instance.SelectTiles(value);
        }

        /// <summary>
        /// Get/Set the selected wire.
        /// </summary>
        public static Wire selectedWire {
            get => instance._selectedWire;
            set => instance.SelectWire(value);
        }

#if false
        /// <summary>
        /// Returns true if the given cell is part of the current selection
        /// </summary>
        /// <param name="cell">Cell to test</param>
        /// <returns>True if the cell is part of the current selection+</returns>
        private bool IsSelected (Cell cell) => hasSelection && (_selection.min == _selection.max ? _selection.min == cell : selectedBounds.Contains(cell));

        private bool IsSelected(Vector3 position) => hasSelection && selectedWorldBounds.ContainsXZ(position);
#endif

        public void ClearSelection()
        {
            if (_selectedWire != null)
                SelectWire(null);

            while (_selectedTiles.Count > 0)
                RemoveSelection(_selectedTiles[0]);

            RefreshInspectorInternal();
        }

#if false
        private void UpdateSelectionGizmo()
        {
            if (_selection.min == _selection.max)
            {
                var cellWorldBounds = puzzle.grid.CellToWorldBounds(_selection.min);
                _selectionGizmo.min = cellWorldBounds.min;
                _selectionGizmo.max = cellWorldBounds.max;
            } else
            {
                _selectionGizmo.min = _puzzle.grid.CellToWorld(_selection.min) - new Vector3(0.5f, 0, 0.5f);
                _selectionGizmo.max = _puzzle.grid.CellToWorld(_selection.max) + new Vector3(0.5f, 0, 0.5f);
            }
        }
#endif

        /// <summary>
        /// Return true if a tile is selected
        /// </summary>
        /// <param name="tile">Tile</param>
        /// <returns>True if selected, false if not</returns>
        private bool IsSelected(Tile tile) => tile.GetComponent<Selected>() != null;

        /// <summary>
        /// Select the top most tile in the given cell
        /// </summary>
        /// <param name="cell"></param>
        public static void SelectTile(Cell cell) => SelectTile(instance.GetTopMostTile(cell));

        /// <summary>
        /// Select the givens tiles
        /// </summary>
        /// <param name="tiles">Tiles to select</param>
        private void SelectTiles(Tile[] tiles)
        {
            ClearSelection();

            if (null == tiles || tiles.Length == 0)
                return;

            foreach (var tile in tiles)
                AddSelection(tile);

            RefreshInspectorInternal();
        }

        /// <summary>
        /// Add a tile to the current selection
        /// </summary>
        /// <param name="tile">Tile to select</param>
        private void AddSelection (Tile tile)
        {
            if (_selectedTiles.Contains(tile))
                return;

            _selectedTiles.Add(tile);
            tile.gameObject.AddComponent<Selected>();

            CameraManager.showSelection = true;

            tile.ShowGizmos(true);

            SelectWire(null);
        }

        /// <summary>
        /// Remove a tile from the current selection
        /// </summary>
        /// <param name="tile">Tile to remove</param>
        private void RemoveSelection (Tile tile)
        {
            if (!_selectedTiles.Contains(tile))
                return;

            SetWiresDark(tile, false);

            Destroy(tile.gameObject.GetComponent<Selected>());

            _selectedTiles.Remove(tile);

            tile.ShowGizmos(false);

            CameraManager.showSelection = _selectedTiles.Count > 0;

            SelectWire(null);
        }

        public static void SelectTile(Tile tile) => instance.SelectTileInternal(tile);

        private void SelectTileInternal(Tile tile)
        {
            // Clear the current selection
            ClearSelection();

#if false
            // Save the inspector state
            if (_selection.tile != null)
            {
                var selected = _selection.tile.GetComponent<Selected>();
                if (null != selected)
                    Destroy(selected);

                _selection.tile.ShowGizmos(false);

                // Make sure the current edit box finishes before we clear the selected tile
                if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inspectorTileName)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                SetWiresDark(_selection.tile, false);
                UpdateInspectorState(_selection.tile);
            }
#endif
            AddSelection(tile);            

#if false
            if (tile == null)
            {
                ClearSelection();

                // Show all wires when no tile is selected
                if (mode == Mode.Logic)
                    _puzzle.ShowWires();
                else
                    _puzzle.HideWires();
            } 
            else
            {
                // Hide all wires in case they were all visible previously and show the selected tiles wires
                _puzzle.HideWires();
                _puzzle.ShowWires(tile);
                RefreshInspectorInternal();
            }
#endif

            RefreshInspectorInternal();
        }

        /// <summary>
        /// Select the given wire
        /// </summary>
        /// <param name="wire">Wire to select</param>
        private void SelectWire(Wire wire)
        {
            // Make sure one of the two tiles from the wire is selected, if not select the input
            if (wire != null && !IsSelected(wire.from.tile) && !IsSelected(wire.to.tile))
                SelectTile(wire.from.tile);

            // Unselect the current wire selection
            if (_selectedWire != null)
                _selectedWire.visuals.selected = false;

            _selectedWire = wire;

            if (_selectedWire != null)
                _selectedWire.visuals.selected = true;

            onSelectedWireChanged?.Invoke(_selectedWire);
        }

        /// <summary>
        /// Fit the current selection rect to the visible tiles within it
        /// </summary>
        private void FitSelection()
        {
            if (!hasSelection)
                return;

            //SelectTiles(GetSelectedTiles());
        }

        private void SelectNextTileUnderCursor()
        {
#if false
            if (selectedTile == null || !_puzzle.grid.CellContainsWorldPoint(selectedTile.cell, _cursorWorld))
            {
                SelectTile(_cursorCell);
                return;
            }

            var cell = _puzzle.grid.WorldToCell(_cursorWorld + new Vector3(0.5f, 0, 0.5f), CellCoordinateSystem.Grid);
            var overlappingTiles = _puzzle.grid.GetTiles(cell, cell).Where(t => _puzzle.grid.CellContainsWorldPoint(t.cell, _cursorWorld)).OrderByDescending(t => t.layer).ToList();
            if (overlappingTiles.Count == 0)
                SelectTile(null);
            else
                SelectTile(overlappingTiles[(overlappingTiles.IndexOf(selectedTile) + 1) % overlappingTiles.Count]);
#endif
        }
    }
}
