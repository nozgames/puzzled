using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        private WireVisuals _dragWire = null;
        private List<Tile> _cursorTiles = null;
        private Tile _pendingSelection = null;

        public static Action<Wire> onSelectedWireChanged;

        private void EnableSelectTool()
        {
            _canvas.onLButtonDown = OnSelectLButtonDown;
            _canvas.onLButtonUp = OnSelectLButtonUp;
            _canvas.onLButtonDragBegin = OnSelectLButtonDragBegin;
            _canvas.onLButtonDrag = OnSelectLButtonDrag;
            _canvas.onLButtonDragEnd = OnSelectLButtonDragEnd;

            _onKey = OnSelectToolKey;
            _getCursor = OnSelectToolGetCursor;

            inspector.SetActive(true);

            _puzzle.ShowWires(!hasSelection);
        }

        private void DisableSelectTool()
        {
            _cursorTiles = null;
        }

        private List<Tile> RaycastTiles (Ray ray) => 
            Physics.RaycastAll(_cursorRay, 100.0f, CameraManager.camera.cullingMask)
                .Select(h => h.collider.GetComponentInParent<Tile>())
                .Where(t => t != null)
                .ToList();

        private void OnSelectLButtonDown (Vector2 position)
        {
            var hit = _cursorTiles;
            if (hit.Count == 0)
                return;

            var hitTile = hit.Last();
            var selectedIndex = _selectedTiles.Count > 0 ? hit.IndexOf(_selectedTiles[0]) : -1;

            // When shift is held use multi-select logic
            if (KeyboardManager.isShiftPressed)
            {
                if (IsSelected(hitTile))
                    RemoveSelection(hitTile);
                else
                    AddSelection(hitTile);
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
                _pendingSelection = hit[(selectedIndex - 1 + hit.Count) % hit.Count];
            }
            // Single select logic
            else
            { 
                SelectTile(hitTile);
            }
        }

        private void OnSelectLButtonUp (Vector2 position)
        {
            if(_pendingSelection != null)
            {
                SelectTile(_pendingSelection);
                _pendingSelection = null;
            }
        }

        private void OnSelectLButtonDragBegin(Vector2 position)
        {
            // Do not allow dragging wires if the selection has more than one tile
            if (_selectedTiles.Count != 1)
                return;

            // Do not allow dragging wires if there is no output on the selected tile
            var tile = _selectedTiles[0];
            if (!tile.hasOutputs)
                return;

            // Since a drag is starting we cancel any pending tile selection
            _pendingSelection = null;

            // Start dragging a wire
            _dragWire = Instantiate(dragWirePrefab, puzzle.transform).GetComponent<WireVisuals>();
            _dragWire.portTypeFrom = PortType.Power;
            _dragWire.portTypeTo = PortType.Power;
            _dragWire.selected = true;
            _dragWire.transform.position = puzzle.grid.CellToWorldBounds(tile.cell).center;
            _dragWire.target = puzzle.grid.CellToWorldBounds(tile.cell).center;

            UpdateCursor();
        }

        private void OnSelectLButtonDrag(Vector2 position, Vector2 delta)
        {
            if (null == _dragWire)
                return;

            _dragWire.target = puzzle.grid.CellToWorldBounds(_cursorCell).center;
        }

        private void OnSelectLButtonDragEnd(Vector2 position)
        {
            if (null == _dragWire)
                return;

            // Stop dragging
            Destroy(_dragWire.gameObject);
            _dragWire = null;

            // Connect to the cell
            Connect(_selectedTiles[0], _cursorCell);

            UpdateCursor();
        }

        private void Connect(Tile tile, Cell cell)
        {
            var group = new Commands.GroupCommand();

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

        private void OnSelectToolKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Delete:
                    if (_selectedTiles.Count == 0)
                        return;

                    var command = new Commands.GroupCommand();
                    foreach (var tile in _selectedTiles)
                        Erase(tile, command);

                    ExecuteCommand(command);
                    break;

                case KeyCode.F:
                    if(hasSelection)
                        Center(selectedTiles, _cameraZoom);
                    break;
            }
        }

        private CursorType OnSelectToolGetCursor(Cell cell)
        {
            _cursorTiles = RaycastTiles(_cursorRay);

            // If dragging a wire..
            if (_dragWire != null)
            {
                // TODO: if over a tile that can be connected to then plus icon else not icon
            }

            return CursorType.Arrow;
        }
    }
}
