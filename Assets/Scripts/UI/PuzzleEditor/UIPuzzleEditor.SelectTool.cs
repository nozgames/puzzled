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

        private bool _dragCancelled = false;
        private WireVisuals _dragWire = null;
        private List<Tile> _cursorTiles = null;
        private Tile _cursorHighlight = null;
        private Vector2 _selectionStart;
        private Vector3 _selectionStartWorld;
        private Cell _selectionStartCell;
        private List<Tile> _savedSelection = new List<Tile>();
        private bool _cursorOverWireGizmo = false;
        private bool _cursorOverWireGizmoAtStart = false;

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

        private Commands.GroupCommand _copyCommand;

        public static Action<Wire> onSelectedWireChanged;

        /// <summary>
        /// Returns true if the select tool is performing a drag operation
        /// </summary>
        public bool isSelectToolDragging => _mode == Mode.Select && (_moveTiles != null || _dragWire != null || _selectionRect.gameObject.activeSelf);

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
            _cursorOverWireGizmo = false;

            inspector.SetActive(true);

            ClearSelection();
        }

        private void DisableSelectTool()
        {
            HilightTile(null);
            _cursorTiles = null;

            ClearSelection();
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
            _cursorOverWireGizmoAtStart = _cursorOverWireGizmo;
            _dragCancelled = false;
        }

        private void OnSelectLButtonUp (Vector2 position)
        {
            if(_dragCancelled)
            {
                _dragCancelled = false;
                UpdateCursor();
                return;
            }

            if (_moveTiles != null)
            {
                EndMove(false);

                return;
            }

            if (_dragWire != null || _selectionRect.gameObject.activeSelf)
            {
                UpdateCursor();
                return;
            }

            var hit = _cursorTiles;
            var hitTile = hit.LastOrDefault();

            // Quick connect / disconnect
            if(KeyboardManager.isCtrlPressed && _inspectorTile != null && hitTile != null)
            {
                if (UI.UIManager.cursor == CursorType.ArrowWithPlus)
                    Connect(_inspectorTile, new Tile[] { hitTile });
                else
                    Disconnect(_inspectorTile, hitTile);

                return;
            }

            if (hitTile == null)
            {
                ClearSelection();
                return;
            }

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
            if (_cursorOverWireGizmoAtStart)
            {
                // Start dragging a wire
                _dragWire = Instantiate(dragWirePrefab, puzzle.transform).GetComponent<WireVisuals>();
                _dragWire.portTypeFrom = PortType.Power;
                _dragWire.portTypeTo = PortType.Power;
                _dragWire.selected = true;
                _dragWire.target = _dragWire.transform.position = _inspectorTile.wireAttach.position;

                UpdateCursor();

                return;
            }

            // If over a selected tile then start moving
            if (_cursorTiles.Any(t => t.isSelected))
            {
                BeginMove();
                return;
            }

            BeginBoxSelect(position);
        }

        private void OnSelectLButtonDrag(Vector2 position, Vector2 delta)
        {
            if (_dragCancelled)
                return;

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
                EndBoxSelection(position);
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
                EndMove(false);
                return;
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
            _cursorOverWireGizmo = false;

            if (_dragCancelled)
                return CursorType.Arrow;

            if (_moveTiles != null)
            {
                HilightTile(null);
                return CursorType.Move;
            }

            // Show arrow for box selection
            if (_selectionRect.gameObject.activeSelf)
            {
                HilightTile(null);
                return CursorType.Arrow;
            }

            // Is the cursor of the wire gizmo?
            _cursorOverWireGizmo = Physics.Raycast(_cursorRay, 100.0f, 1 << _wireGizmo.gameObject.layer);

            // If the wire gizmo is active then hit test it
            if (_dragWire == null && _cursorOverWireGizmo)
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

            // Quick connect / disconnect
            if(KeyboardManager.isCtrlPressed && _inspectorTile != null)
            {
                if (_cursorTiles.Count == 0)
                    return CursorType.ArrowWithNot;

                if (_inspectorTile.IsConnectedTo(_cursorTiles.LastOrDefault()))
                    return CursorType.ArrowWithMinus;
                
                if(_inspectorTile.CanConnectTo(_cursorTiles.LastOrDefault(), false))
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

        private void BeginBoxSelect(Vector3 position)
        {
            _savedSelection.Clear();
            _savedSelection.AddRange(_selectedTiles);

            // Show the box selection 
            _selectionRect.gameObject.SetActive(true);

            // Start with the new box selection
            UpdateBoxSelection(position);
        }

        private void EndBoxSelection(Vector3 position, bool cancel=false)
        {
            if (!cancel)
            {
                UpdateBoxSelection(position);

                // Apply the box selection
                ClearSelection();
                foreach(var tile in _boxSelectionTiles)
                    AddTileToSelectedTiles(tile);

                UpdateWireVisibility();

                RefreshInspectorInternal();
            } else
            {
                SelectTiles(_savedSelection.ToArray());
            }

            // Clear the highlight on box selection tiles
            foreach (var tile in _boxSelectionTiles)
                tile.editor.isHighlighted = false;

            _selectionRect.gameObject.SetActive(false);
            _savedSelection.Clear();
            _boxSelectionTiles.Clear();
        }


        private Tile[] CloneTiles(Tile[] tiles)
        {
            var cloned = new Tile[tiles.Length];
            for (int i = 0; i < tiles.Length; i++)
            {
                cloned[i] = puzzle.InstantiateTile(DatabaseManager.GetTile(tiles[i].guid), Cell.invalid);
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                var sourcePorts = tiles[i].GetPorts();
                for (int j = 0; j < sourcePorts.Length; j++)
                {
                    var sourcePort = sourcePorts[j];
                    if (sourcePort.flow != PortFlow.Output)
                        continue;

                    foreach (var sourceWire in sourcePort.wires)
                    {
                        var fromPortTileIndex = 0;
                        for (; fromPortTileIndex < tiles.Length && tiles[fromPortTileIndex] != sourceWire.from.tile; fromPortTileIndex++)
                            ;
                        if (fromPortTileIndex >= tiles.Length)
                            continue;

                        var toPortTileIndex = 0;
                        for (; toPortTileIndex < tiles.Length && tiles[toPortTileIndex] != sourceWire.to.tile; toPortTileIndex++)
                            ;
                        if (toPortTileIndex >= tiles.Length)
                            continue;

                        var fromPort = cloned[fromPortTileIndex].GetPropertyValue<Port>(sourceWire.from.port.name);
                        if (null == fromPort)
                            continue;

                        var toPort = cloned[toPortTileIndex].GetPropertyValue<Port>(sourceWire.to.port.name);
                        if (null == toPort)
                            continue;

                        // Copy wire options
                        var wire = puzzle.InstantiateWire(fromPort, toPort);
                        if (sourceWire.from.hasOptions)
                            for (int optionIndex = 0; optionIndex < sourceWire.from.options.Length; optionIndex++)
                                wire.from.SetOption(optionIndex, sourceWire.from.GetOption(optionIndex));

                        if (sourceWire.to.hasOptions)
                            for (int optionIndex = 0; optionIndex < sourceWire.to.options.Length; optionIndex++)
                                wire.to.SetOption(optionIndex, sourceWire.to.GetOption(optionIndex));
                    }
                }

                foreach (var property in tiles[i].properties)
                    if (property.type != TilePropertyType.Port && property.editable.serialized)
                        property.SetValue(cloned[i], property.GetValue(tiles[i]));
            }

            return cloned;
        }

        private void BeginMove ()
        {
            _moveTiles = _selectedTiles.ToArray();
            _moveCells = _moveTiles.Select(t => t.cell).ToArray();

            // Hide the wire gizmo while moving tiles
            _wireGizmo.gameObject.SetActive(false);

            if (KeyboardManager.isAltPressed)
            {
                _moveTiles = CloneTiles(_moveTiles);

                _copyCommand = new Commands.GroupCommand();
                foreach (var tile in _moveTiles)
                    _copyCommand.Add(new Commands.TileAddCommand(DatabaseManager.GetTile(tile.guid), Cell.invalid, tile));

                _moveCommand = CreateMoveCommand(_moveTiles, _moveCells, _moveOffset);
                _moveCommand.Execute();

                SelectTiles(_moveTiles);
            }

            UpdateCursor();
        }

        private void EndMove(bool cancel=false)
        {
            if (_moveCommand != null)
            {
                _moveCommand.Undo();

                var group = false;
                if (_copyCommand != null)
                {
                    group = true;
                    _copyCommand.Undo();

                    if(!cancel)
                        ExecuteCommand(_copyCommand);
                    _copyCommand = null;
                }

                if(!cancel)
                    ExecuteCommand(_moveCommand, group);

                _moveCommand = null;

                UpdateWireGizmo();
            }

            _moveTiles = null;
            _moveCells = null;

            LightmapManager.Render();
        }

        private void CancelDrag ()
        {
            _dragCancelled = true;

            // Cancel drag wire
            if (_dragWire != null)
            {
                Destroy(_dragWire.gameObject);
                _dragWire = null;
            }

            // Cancel box select
            if (_selectionRect.gameObject.activeSelf)
            {
                EndBoxSelection(Vector3.zero, true);
            }

            // Cancel Move
            if (_moveTiles != null)
                EndMove(true);

            UpdateCursor();
        }

        /// <summary>
        /// Disconnect one tile from another, this will disconnect all connections that match the two tiles
        /// </summary>
        private void Disconnect(Tile from, Tile to)
        {
            var group = new Commands.GroupCommand();
            var outputs = from.GetPorts(PortFlow.Output);
            foreach (var output in outputs)
                foreach (var wire in output.wires)
                {
                    var connection = wire.GetOppositeConnection(output);
                    if (connection.tile != to)
                        continue;

                    group.Add(new Commands.WireDestroyCommand(wire));
                }

            if (group.hasCommands)
                ExecuteCommand(group);

            UpdateCursor();
        }

    }
}
