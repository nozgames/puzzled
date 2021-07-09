using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;
using Puzzled.Editor;
using System.Collections.Generic;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        private WireVisuals dragWire = null;
        private bool logicCycleSelection = false;
        private bool _allowLogicDrag = false;

        public static Action<Wire> onSelectedWireChanged;

        private void EnableLogicTool()
        {
            _canvas.onLButtonDown = OnLogicLButtonDown;
            _canvas.onLButtonUp = OnLogicLButtonUp;
            _canvas.onLButtonDragBegin = OnLogicLButtonDragBegin;
            _canvas.onLButtonDrag = OnLogicLButtonDrag;
            _canvas.onLButtonDragEnd = OnLogicLButtonDragEnd;

            _onKey = OnLogicKey;
            _getCursor = OnLogicGetCursor;

            inspector.SetActive(true);

            logicCycleSelection = false;

            // Show all wires when logic tool is enabled
            if(selectedTile == null)
                _puzzle.ShowWires(true);
        }

        private void DisableLogicTool()
        {            
        }

        private void OnInspectorTileNameChanged(string name)
        {
            if (selectedTile == null)
                return;

            name = name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                inspectorTileName.SetTextWithoutNotify(selectedTile.name);
                return;
            }

            if (name == selectedTile.name)
                return;

            ExecuteCommand(new Editor.Commands.TileRenameCommand(selectedTile, name));
        }

        private void OnLogicLButtonDown(Vector2 position)
        {
            // Ensure the cell being dragged is the selected cell
            var cell = _cursorCell;

            _allowLogicDrag = false;

            // QuickConnect mode
            if (selectedTile != null && KeyboardManager.isShiftPressed)
            {
                Connect(selectedTile, cell);
                return;
            }

            if (selectedTile != null && KeyboardManager.isCtrlPressed)
            {
                Disconnect(selectedTile, cell);
                return;
            }

            var hit = Physics.RaycastAll(_cursorRay, 100.0f, (int)0x7FFFFFFF);
            Tile hitTile = null;
            if(hit.Length > 0)
                hitTile = hit.Select(h => h.collider.GetComponentInParent<Tile>()).Where(t => t != null).FirstOrDefault();
            if (hitTile == null)
                hitTile = GetTopMostTile(cell, TileLayer.InvisibleStatic);

            // Handle no selection or selecting a new tile
            if (selectedTile == null || hitTile != selectedTile)  //  !_puzzle.grid.CellContainsWorldPoint(selectedTile.cell, _cursorWorld))
            {
                SelectTile(hitTile);
                logicCycleSelection = false;
            }
            else
                logicCycleSelection = true;

            _allowLogicDrag = selectedTile != null && selectedTile.hasOutputs;
        }

        private void OnLogicLButtonUp(Vector2 position)
        {
            if (null != dragWire || selectedTile==null || !logicCycleSelection)
                return;

            SelectNextTileUnderCursor();
        }

        private void OnLogicLButtonDragBegin(Vector2 position)
        {
            if (!_allowLogicDrag)
                return;

            if (selectedTile == null || !selectedTile.hasOutputs)
                return;

            dragWire = Instantiate(dragWirePrefab, puzzle.transform).GetComponent<WireVisuals>();
            dragWire.portTypeFrom = PortType.Power;
            dragWire.portTypeTo = PortType.Power;
            dragWire.selected = true;
            dragWire.transform.position = puzzle.grid.CellToWorldBounds(selectedTile.cell).center;
            dragWire.target = puzzle.grid.CellToWorldBounds(selectedTile.cell).center;

            UpdateCursor();
        }

        private void OnLogicLButtonDrag(Vector2 position, Vector2 delta)
        {
            if (null == dragWire)
                return;

            dragWire.target = puzzle.grid.CellToWorldBounds(_cursorCell).center;
        }

        private void OnLogicLButtonDragEnd(Vector2 position)
        {
            if (null == dragWire)
                return;

            // Stop dragging
            Destroy(dragWire.gameObject);
            dragWire = null;

            // Connect to the cell
            Connect(selectedTile, _cursorCell);

            UpdateCursor();
        }

        private void Connect(Tile tile, Cell cell)
        {
            var group = new Editor.Commands.GroupCommand();

            if (!tile.CanConnectTo(cell))
                return;

            // Get all in the given cell that we can connect to
            var tiles = puzzle.grid.GetTiles(cell).Where(t => tile.CanConnectTo(t, false)).ToArray();
            if (tiles.Length == 0)
                return;

            if(tiles.Length == 1)
            {
                ChoosePort(tile, tiles[0], (from, to) => {
                    ExecuteCommand(new Editor.Commands.WireAddCommand(from, to), false, (cmd) => {
                        selectedWire = (cmd as Editor.Commands.WireAddCommand).addedWire;
                    });
                });
            }
            else
            {
                ChooseTileConnection(tiles, (target) => {
                    ChoosePort(tile, target, (from, to) => {
                        ExecuteCommand(new Editor.Commands.WireAddCommand(from, to));
                    });
                });
            }
        }

        private void Disconnect(Tile tile, Cell cell)
        {
            var group = new Editor.Commands.GroupCommand();
            var outputs = tile.GetPorts(PortFlow.Output);
            foreach(var output in outputs)
                foreach(var wire in output.wires)
                {
                    var connection = wire.GetOppositeConnection(output);
                    if(connection.cell != cell)
                        continue;

                    group.Add(new Editor.Commands.WireDestroyCommand(wire));
                }

            if(group.hasCommands)
                ExecuteCommand(group);

            UpdateCursor();
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

        private void OnLogicKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Delete:
                    if (selectedTile != null)
                        ExecuteCommand(Erase(selectedTile));
                    break;

                case KeyCode.F:
                    if(selectedTile != null)
                        Center(selectedTile.cell, _cameraZoom);
                    break;
            }
        }

        private CursorType OnLogicGetCursor(Cell cell)
        {
            // When shift is pressed it means "QuickConnect" mode
            if ((KeyboardManager.isShiftPressed && selectedTile != null) || dragWire != null)
            {
                var canConnect = selectedTile.CanConnectTo(cell, false);
                if (dragWire)
                    dragWire.highlight = canConnect;

                return canConnect ? CursorType.ArrowWithPlus : CursorType.ArrowWithNot;
            }

            // When ctrl is pressed it means "QuickDisconnect" mode
            if (KeyboardManager.isCtrlPressed && selectedTile != null)
                return selectedTile.IsConnectedTo(cell) ? CursorType.ArrowWithMinus : CursorType.ArrowWithNot;

            return CursorType.Arrow;
        }
    }
}
