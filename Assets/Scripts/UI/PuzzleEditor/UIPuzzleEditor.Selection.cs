using System.Linq;
using UnityEngine;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        private struct Selection
        {
            public Cell min;
            public Cell max;
            public Vector2Int size;
            public Tile tile;
            public Wire wire;
        }

        private Selection _selection;

        public static Tile selectedTile {
            get => instance._selection.tile;
            set => instance.SelectTile(value);
        }

        public static Wire selectedWire {
            get => instance._selection.wire;
            set => instance.SelectWire(value);
        }

        /// <summary>
        /// Returns the selected bounds.
        /// </summary>
        public static CellBounds selectedBounds => new CellBounds(instance._selection.min, instance._selection.max);

        public static Bounds selectedWorldBounds {
            get {
                var bounds = instance._puzzle.grid.CellToWorldBounds(instance._selection.min);
                bounds.Encapsulate(instance._puzzle.grid.CellToWorldBounds(instance._selection.max));
                return bounds;
            }
        }
            
        public bool hasSelection => _selectionGizmo.gameObject.activeSelf;

        /// <summary>
        /// Returns true if the given cell is part of the current selection
        /// </summary>
        /// <param name="cell">Cell to test</param>
        /// <returns>True if the cell is part of the current selection+</returns>
        private bool IsSelected (Cell cell) => hasSelection && (_selection.min == _selection.max ? _selection.min == cell : selectedBounds.Contains(cell));

        private bool IsSelected(Vector3 position) => hasSelection && selectedWorldBounds.ContainsXZ(position);

        /// <summary>
        /// Return and array of all selected tiles
        /// </summary>
        private Tile[] GetSelectedTiles() =>
            hasSelection ?
                puzzle.grid.GetTiles(selectedBounds).Where(t => IsLayerVisible(t.layer)).ToArray() :
                null;

        public void ClearSelection()
        {
            if (_selection.wire != null)
                SelectWire(null);

            if (_selection.tile != null)
                SelectTile(null);

            _selection.min = _selection.max = Cell.invalid;
            _selection.size = Vector2Int.zero;
            _selectionGizmo.gameObject.SetActive(false);
        }

        public void SelectRect(Cell min, Cell max) => SelectRect(new CellBounds(min, max));

        public void SelectRect(CellBounds cellBounds)
        {
            _selection.min = cellBounds.min;
            _selection.max = cellBounds.max;
            _selection.size = _selection.max - _selection.min;

            _selectionGizmo.gameObject.SetActive(true);
            UpdateSelectionGizmo();
        }

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

        private void SelectTile(Cell cell) => SelectTile(GetTopMostTile(cell));

        private void SelectTile(Tile tile)
        {
            // Save the inspector state
            if (_selection.tile != null)
            {
                _selection.tile.ShowGizmos(false);

                // Make sure the current edit box finishes before we clear the selected tile
                if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inspectorTileName)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                SetWiresDark(_selection.tile, false);
                UpdateInspectorState(_selection.tile);
            }

            _selection.tile = tile;

            HideCameraEditor();

            if (tile == null)
            {
                ClearSelection();
                _inspectorContent.transform.DetachAndDestroyChildren();
                _inspectorContent.SetActive(false);
                _inspectorHeader.SetActive(false);
                _inspectorEmpty.SetActive(true);

                // Show all wires when no tile is selected
                if (mode == Mode.Logic)
                    _puzzle.ShowWires();
                else
                    _puzzle.HideWires();
            } 
            else
            {
                _inspectorEmpty.SetActive(false);
                _inspectorContent.SetActive(true);
                _inspectorHeader.SetActive(true);
                inspectorTileName.SetTextWithoutNotify(tile.name);
                _inspectorTileType.text = $"<{_selection.tile.info.displayName}>";

                var rotation = _selection.tile.GetProperty("rotation");
                _inspectorRotateButton.gameObject.SetActive(rotation != null);

                _inspectorTilePreview.sprite = DatabaseManager.GetPreview(tile.guid);

                // Update the selection
                _selection.min = _selection.max = tile.cell;
                _selection.size = Vector2Int.one;
                _selectionGizmo.gameObject.SetActive(true);
                UpdateSelectionGizmo();

                // Hide all wires in case they were all visible previously and show the selected tiles wires
                _puzzle.HideWires();
                _puzzle.ShowWires(tile);
                RefreshInspectorInternal();

                // If the tile is a camera then open the camera editor as well
                var gameCamera = tile.GetComponent<GameCamera>();
                if (gameCamera != null)
                    ShowCameraEditor(gameCamera);

                _selection.tile.ShowGizmos(true);
            }

            // Clear wire selection if the selected wire does not connect to the newly selected tile
            if (selectedWire != null && selectedWire.from.tile != tile && selectedWire.to.tile != tile)
                SelectWire(null);
        }

        /// <summary>
        /// Select the given wire
        /// </summary>
        /// <param name="wire">Wire to select</param>
        private void SelectWire(Wire wire)
        {
            // Make sure one of the two tiles from the wire is selected, if not select the input
            if (wire != null && _selection.tile != wire.from.tile && _selection.tile != wire.to.tile)
                SelectTile(wire.from.tile);

            if (_selection.wire != null)
                _selection.wire.visuals.selected = false;

            _selection.wire = wire;

            if (_selection.wire != null)
                _selection.wire.visuals.selected = true;

            onSelectedWireChanged?.Invoke(_selection.wire);
        }

        /// <summary>
        /// Fit the current selection rect to the visible tiles within it
        /// </summary>
        private void FitSelection()
        {
            if (!hasSelection)
                return;

            SelectTiles(GetSelectedTiles());
        }

        private void SelectNextTileUnderCursor()
        {
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
        }

        /// <summary>
        /// Select the givens tiles
        /// </summary>
        /// <param name="tiles">Tiles to select</param>
        private void SelectTiles (Tile[] tiles)
        {
            if (null == tiles)
                return;

            ClearSelection();

            if (tiles.Length == 0)
                return;

            if (tiles.Length == 1)
            {
                SelectTile(tiles[0]);
                return;
            }

            var min = tiles[0].cell;
            var max = min;
            foreach (var tile in tiles)
            {
                min = Cell.Min(min, tile.cell);
                max = Cell.Max(max, tile.cell);
            }

            SelectRect(min, max);
        }
    }
}
