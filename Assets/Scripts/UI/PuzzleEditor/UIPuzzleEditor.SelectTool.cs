﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        [Header("SelectTool")]
        [SerializeField] private RectTransform _selectionRect = null;

        private WireVisuals _dragWire = null;
        private List<Tile> _cursorTiles = null;
        private Tile _cursorHighlight = null;
        private Vector2 _selectionStart;
        private Vector3 _selectionStartWorld;
        private Cell _selectionStartCell;
        private List<Tile> _savedSelection = new List<Tile>();

        /// <summary>
        /// Tiles being moved
        /// </summary>
        private Tile[] _moveTiles;

        /// <summary>
        /// Original cells of all tiles being moved
        /// </summary>
        private Cell[] _moveCells;

        /// <summary>
        /// Current offset from the selection start
        /// </summary>
        private Vector2Int _moveOffset;

        /// <summary>
        /// Last move command issued
        /// </summary>
        private Commands.Command _moveCommand;


        public static Action<Wire> onSelectedWireChanged;

        private void EnableSelectTool()
        {
            _canvas.onLButtonDown = OnSelectLButtonDown;
            _canvas.onLButtonUp = OnSelectLButtonUp;
            _canvas.onLButtonDragBegin = OnSelectLButtonDragBegin;
            _canvas.onLButtonDrag = OnSelectLButtonDrag;
            _canvas.onLButtonDragEnd = OnSelectLButtonDragEnd;
            _canvas.onExit = OnSelectToolExitCanvas;

            _onKey = OnSelectToolKey;
            _onKeyModifiers = OnSelectToolKeyModifiers;
            _getCursor = OnSelectToolGetCursor;

            inspector.SetActive(true);

            _puzzle.ShowWires(!hasSelection);
        }

        private void DisableSelectTool()
        {
            HilightTile(null);
            _cursorTiles = null;
        }

        private List<Tile> RaycastTiles (Ray ray) => 
            Physics.RaycastAll(_cursorRay, 100.0f, CameraManager.camera.cullingMask)
                .OrderByDescending(h => h.distance)
                .Select(h => h.collider.GetComponentInParent<Tile>())
                .Where(t => t != null)
                .ToList();

        private void OnSelectLButtonDown (Vector2 position)
        {
            // Initial box selection
            _selectionStart = position;
            _selectionStartWorld = _cursorWorld;
            _selectionStartCell = _cursorCell;
            _selectionRect.offsetMin = position;
            _selectionRect.offsetMax = position;
        }

        private void OnSelectLButtonUp (Vector2 position)
        {
            if (_moveTiles != null)
            {
                if (_moveCommand != null)
                {
                    _moveCommand.Undo();

#if false
                    var group = false;
                    if (_copyCommand != null)
                    {
                        group = true;
                        _copyCommand.Undo();
                        ExecuteCommand(_copyCommand);
                        _copyCommand = null;
                    }
                    ExecuteCommand(_moveCommand, true);
#else
                    ExecuteCommand(_moveCommand, false);
#endif

                    _moveCommand = null;

                    //SelectTiles(_moveTiles);
                }

                return;
            }

            if (_dragWire != null || _selectionRect.gameObject.activeSelf)
                return;

            var hit = _cursorTiles;
            if (hit.Count == 0)
            {
                ClearSelection();
                return;
            }

            var hitTile = hit.Last();
            var selectedIndex = _selectedTiles.Count > 0 ? hit.IndexOf(_selectedTiles[0]) : -1;

            // When shift is held use multi-select logic
            if (KeyboardManager.isShiftPressed)
            {
                if (hitTile.isSelected)
                    RemoveSelection(hitTile);
                else
                    AddSelection(hitTile);

                RefreshInspectorInternal();
            }
            // If there is only one selected tile and that tile was hit then we need to cycle
            // selection to the next lowest tile.
            else if (_selectedTiles.Count == 1 && selectedIndex != -1)
            {
                // Selected the same tile so just ignore it
                if (hit.Count == 1)
                    return;

                // Set the pending selection to the tile under the current one selected.  This pending
                // selection will be realized if no drag occurs and the mouse is released.
                SelectTile(hit[(selectedIndex - 1 + hit.Count) % hit.Count]);
            }
            // Single select logic
            else
            {
                SelectTile(hitTile);
            }

            UpdateCursor();
        }        

        private void OnSelectLButtonDragBegin(Vector2 position)
        {
            // Drag a wire from the wire gizmo
            if(IsOverWireGizmo(_selectionStartWorld))
            {
                // Start dragging a wire
                _dragWire = Instantiate(dragWirePrefab, puzzle.transform).GetComponent<WireVisuals>();
                _dragWire.portTypeFrom = PortType.Power;
                _dragWire.portTypeTo = PortType.Power;
                _dragWire.selected = true;
                _dragWire.target = _dragWire.transform.position = puzzle.grid.CellToWorldBounds(_inspectorTile.cell).center;

                UpdateCursor();

                return;
            }

            // If over a selected tile then start moving
            if (_cursorTiles.Any(t => t.isSelected))
            {
                _moveTiles = _selectedTiles.ToArray();
                _moveCells = _moveTiles.Select(t => t.cell).ToArray();
                return;
            }

            // TODO: if near outside edge of selection allow rotate (single tile only for now)
            // TODO: if dragged from on top of a selected tile then perform a move
            // TODO: if dragged from the wire manipulator then start a wire drag

            // Save the selection for shift box drag operations
            _savedSelection.Clear();
            _savedSelection.AddRange(_selectedTiles);

            // Show the box selection 
            _selectionRect.gameObject.SetActive(true);

            // Start with the new box selection
            UpdateBoxSelection(position);
        }

        private void OnSelectLButtonDrag(Vector2 position, Vector2 delta)
        {
            // Update wire drag
            if (null != _dragWire)
            {
                _dragWire.target = _cursorWorld; 
                return;
            }

            // Update box selection
            if (_selectionRect.gameObject.activeSelf)
            {
                UpdateBoxSelection(position);
                return;
            }

            // Update Move
            if(_moveTiles != null)
            {
                // Calculate the move offset and early out if there is no difference
                var offset = (new Cell(_cursorCell, CellEdge.None) - _selectionStartCell);
                if (offset == _moveOffset)
                    return;

                // If there was a previous move command undo it to revert back to the pre move state
                if (_moveCommand != null)
                {
                    _moveCommand.Undo();
                    _moveCommand = null;
                }

                // Move the tiles using the new offset
                _moveOffset = offset;

                _moveCommand = CreateMoveCommand(_moveTiles, _moveCells, _moveOffset);
                _moveCommand.Execute();

                LightmapManager.Render();

                UpdateCursor();
            }
        }

        private void OnSelectLButtonDragEnd(Vector2 position)
        {
            // End Box selection
            if(_selectionRect.gameObject.activeSelf)
            {
                UpdateBoxSelection(position);
                RefreshInspectorInternal();
                _selectionRect.gameObject.SetActive(false);
                _savedSelection.Clear();
                _boxSelectionTiles.Clear();
                _boxSelectionTilesOutside.Clear();
                return;
            }

            // End Wire drag
            if (null != _dragWire)
            {
                // Stop dragging
                Destroy(_dragWire.gameObject);
                _dragWire = null;

                // Connect to the cursor tiles
                Connect(_inspectorTile, _cursorTiles.Where(t => _inspectorTile.CanConnectTo(t, false)).ToArray());

                UpdateCursor();
                return;
            }

            // End move
            if(null != _moveTiles)
            {
                _moveTiles = null;
            }
        }

        private void Connect(Tile tile, Tile[] tiles)
        {
            if (tiles.Length == 0)
                return;

            if (tiles.Length == 1)
            {
                ChoosePort(tile, tiles[0], (from, to) => {
                    ExecuteCommand(new Commands.WireAddCommand(from, to), false, (cmd) => {
                        selectedWire = (cmd as Commands.WireAddCommand).addedWire;
                    });
                });
            } else
            {
                ChooseTileConnection(tiles, (target) => {
                    ChoosePort(tile, target, (from, to) => {
                        ExecuteCommand(new Commands.WireAddCommand(from, to));
                    });
                });
            }
        }

        private void SetWiresDark (Tile tile, bool dark)
        {
            if (null == tile || null == tile.properties)
                return;

            foreach (var property in tile.properties)
                if (property.type == TilePropertyType.Port)
                    foreach (var wire in property.GetValue<Port>(tile).wires)
                        wire.visuals.highlight = !dark;
        }

        private void OnSelectToolKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Delete:
                    ExecuteCommand(Erase(_selectedTiles.ToArray()));
                    break;

                case KeyCode.F:
                    if(hasSelection)
                        Center(selectedTiles, _cameraZoom);
                    break;
            }
        }

        private void OnSelectToolKeyModifiers(bool shift, bool ctrl, bool alt)
        {
            // Update box selection whenever modifiers change if it is active
            if (_selectionRect.gameObject.activeSelf)
                UpdateBoxSelection(shift);
        }

        /// <summary>
        /// Returns true if the cursor is over the wire gizmo
        /// </summary>
        private bool IsOverWireGizmo(Vector3 position) => _wireGizmo.activeSelf && (position - _wireGizmo.transform.position).magnitude <= _wireGizmo.transform.localScale.x * 0.5f;

        /// <summary>
        /// Set the higlight tile
        /// </summary>
        private void HilightTile(Tile highlight)
        {
            if (highlight == _cursorHighlight)
                return;

            if (_cursorHighlight != null)
                _cursorHighlight.editor.isHighlighted = false;

            _cursorHighlight = highlight;

            if (_cursorHighlight != null)
                _cursorHighlight.editor.isHighlighted = true;
        }

        private CursorType OnSelectToolGetCursor(Cell cell)
        {
            if (_moveTiles != null)
                return CursorType.Move;

            // Show arrow for box selection
            if (_selectionRect.gameObject.activeSelf)
            {
                HilightTile(null);
                return CursorType.Arrow;
            }

            // If the wire gizmo is active then hit test it
            if (_dragWire == null && IsOverWireGizmo(_cursorWorld))
            {
                // TODO: wire crosshair?
                HilightTile(null);
                return CursorType.ArrowWithPlus;
            }

            _cursorTiles = RaycastTiles(_cursorRay);

            HilightTile(_cursorTiles.LastOrDefault());

            // If dragging a wire..
            if (_dragWire != null)
            {
                // Can the inspector tile connect to any of the cursor tiles?
                foreach (var cursorTile in _cursorTiles)
                    if (_inspectorTile.CanConnectTo(cursorTile, false))
                        return CursorType.ArrowWithPlus;

                return CursorType.ArrowWithNot;
            }

            // Move cursor when over a tile that is already selected
            if (_cursorTiles.Any(t => t.isSelected))
                return CursorType.ArrowWithMove;

            return CursorType.Arrow;
        }

        private void OnSelectToolExitCanvas()
        {            
            HilightTile(null);
        }

        /// <summary>
        /// Create a command to move tiles from the current cell to a new cell using an offset
        /// </summary>
        /// <param name="tiles">List of tiles to move</param>
        /// <param name="offset">Offset to move tiles by</param>
        /// <returns>Command used to move the tiles</returns>
        private Commands.Command CreateMoveCommand(Tile[] tiles, Cell[] cells, Vector2Int offset)
        {
            var group = new Commands.GroupCommand();

            // Unlink all tiles being moved
            foreach (var tile in tiles)
                group.Add(new Commands.TileMoveCommand(tile, Cell.invalid));

            // Destroy any tiles in the target that overlap the tiles being moved
            for (int i = 0; i < cells.Length; i++)
            {
                var existing = puzzle.grid.CellToTile(cells[i] + offset, tiles[i].layer);
                if (null == existing || tiles.Contains(existing))
                    continue;

                Erase(existing, group);
            }

            // Link the new tiles in the moved position
            for (int i = 0; i < cells.Length; i++)
                group.Add(new Commands.TileMoveCommand(tiles[i], cells[i] + offset, Cell.invalid));

            return group;
        }
    }
}
