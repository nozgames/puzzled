using System.Linq;
using UnityEngine;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor 
    {
        private enum MoveState
        {
            None,
            Moving,
            Selecting
        }

        /// <summary>
        /// Tiles being moved
        /// </summary>
        private Tile[] _moveTiles;

        private Cell[] _moveCells;

        /// <summary>
        /// Anchor cell for moving tiles
        /// </summary>
        private Cell _moveAnchor;

        /// <summary>
        /// Current offset from the anchor
        /// </summary>
        private Vector2Int _moveOffset;

        /// <summary>
        /// Mask of layers participating in the move
        /// </summary>
        private uint _moveLayerMask;

        /// <summary>
        /// Last move command issued
        /// </summary>
        private Editor.Commands.Command _moveCommand;

        private Editor.Commands.GroupCommand _copyCommand;

        /// <summary>
        /// Current move tool state
        /// </summary>
        private MoveState _moveDragState = MoveState.None;

        private void EnableMoveTool ()
        {
            canvas.onLButtonDown = OnMoveToolLButtonDown;
            canvas.onLButtonUp = OnMoveToolLButtonUp;
            canvas.onLButtonDragBegin = OnMoveToolDragBegin;
            canvas.onLButtonDrag = OnMoveToolDrag;
            canvas.onLButtonDragEnd = OnMoveToolDragEnd;

            inspector.SetActive(true);

            _onKey = OnMoveKey;
            _moveDragState = MoveState.None;

            if(selectedTile == null)
                puzzle.HideWires();

            _getCursor = OnMoveGetCursor;
        }

        private void DisableMoveTool()
        {
            if (hasSelection && !selectedTile)
                ClearSelection();
        }

        private void OnMoveToolLButtonDown(Vector2 position)
        {
            _moveAnchor = _cursorCell;            
        }

        private void OnMoveToolDragBegin(Vector2 position)
        {
            Debug.Assert(_moveDragState == MoveState.None);

            // If dragging a selected tile then drag only that tile
            if (selectedTile != null && selectedTile.cell == _moveAnchor)
            {
                _moveTiles = new[] { selectedTile };
                _moveCells = new[] { selectedTile.cell };
                _moveDragState = MoveState.Moving;
            } 
            // If dragging from a selected cell then start moving
            else if(IsSelected(_cursorWorld))
            {
                _moveAnchor = _cursorCell;
                _moveTiles = GetSelectedTiles();
                _moveCells = _moveTiles.Select(t => t.cell).ToArray();
                _moveDragState = MoveState.Moving;
                _moveLayerMask = GetVisibleLayerMask();
                _moveOffset = Vector2Int.zero;

                if (KeyboardManager.isAltPressed)
                {
                    _moveTiles = CloneTiles(_moveTiles);

                    _copyCommand = new Editor.Commands.GroupCommand();
                    foreach (var tile in _moveTiles)
                        _copyCommand.Add(new Editor.Commands.TileAddCommand(DatabaseManager.GetTile(tile.guid), Cell.invalid, tile));

#if false
                    foreach (var tile in _moveTiles)
                        foreach (var port in tile.GetPorts().Where(p => p.flow == PortFlow.Output))
                            foreach (var wire in port.wires)
                                _copyCommand.Add(new Editor.Commands.WireAddCommand(wire));
#endif

                    _moveCommand = CreateMoveCommand (_moveTiles, _moveCells, _moveOffset, _moveLayerMask);
                    _moveCommand.Execute();
                    //_copyCommand.Execute();
                }
            } 
            // Start dragging a new selection
            else
            {
                _moveAnchor = _moveAnchor.ConvertTo(CellCoordinateSystem.Grid);
                _moveDragState = MoveState.Selecting;
            }

            UpdateCursor();
        }

        private void OnMoveToolDragEnd(Vector2 position)
        {
        }

        /// <summary>
        /// Create a command to move tiles from the current cell to a new cell using an offset
        /// </summary>
        /// <param name="tiles">List of tiles to move</param>
        /// <param name="offset">Offset to move tiles by</param>
        /// <param name="layerMask">Layers to include </param>
        /// <returns>Command used to move the tiles</returns>
        private Editor.Commands.Command CreateMoveCommand (Tile[] tiles, Cell[] cells, Vector2Int offset, uint layerMask, Editor.Commands.GroupCommand group = null)
        {
            if(null == group)
                group = new Editor.Commands.GroupCommand();

            // Unlink all tiles being moved
            foreach (var tile in tiles)
                group.Add(new Editor.Commands.TileMoveCommand(tile, Cell.invalid));

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
                group.Add(new Editor.Commands.TileMoveCommand(tiles[i], cells[i] + offset, Cell.invalid));

            return group;
        }

        /// <summary>
        /// Create a command to copy/move tiles from the current cell to a new cell using an offset
        /// </summary>
        /// <param name="tiles">List of tiles to move</param>
        /// <param name="offset">Offset to move tiles by</param>
        /// <param name="layerMask">Layers to include </param>
        /// <returns>Command used to move the tiles</returns>
        private Editor.Commands.Command CreateCopyMoveCommand (Tile[] source, Tile[] copies, Vector2Int offset, uint layerMask, Editor.Commands.GroupCommand group = null)
        {
            if (source.Length != copies.Length)
                return null;

            if (null == group)
                group = new Editor.Commands.GroupCommand();

            // Destroy any tiles in the target that overlap the tiles being moved
            for (int i = 0; i < source.Length; i++)
                Erase(source[i], group);

            // Link the new tiles in the moved position
            for (int i = 0; i < copies.Length; i++)
                group.Add(new Editor.Commands.TileMoveCommand(copies[i], source[i].cell + offset, Cell.invalid));

            return group;
        }


        private void OnMoveToolDrag(Vector2 position, Vector2 delta)
        {
            switch (_moveDragState)
            {
                case MoveState.Selecting:
                {
                    SelectRect(_moveAnchor, _cursorCell);
                    break;
                }

                case MoveState.Moving:
                {
                    // Calculate the move offset and early out if there is no difference
                    var offset = Vector2Int.zero;
                    if (selectedTile != null)
                        offset = _cursorCell - _moveAnchor;
                    else
                        offset = (new Cell(_cursorCell, CellEdge.None) - _moveAnchor);

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
                    _moveCommand = CreateMoveCommand(_moveTiles, _moveCells, _moveOffset, _moveLayerMask);
                    _moveCommand.Execute();

                    // Update the selection to reflect the move
                    SelectTiles(_moveTiles);

                    UpdateCursor();
                    break;
                }
                default:
                    break;

            }
        }

        private void OnMoveToolLButtonUp (Vector2 position)
        {
            switch (_moveDragState)
            {
                // If a cell was clicked but never dragged then cycle select the tile on that cell
                case MoveState.None:
                {
                    SelectNextTileUnderCursor();
                    break;
                }
                
                case MoveState.Moving:
                {
                    if (_moveTiles == null)
                        break;

                    if (_moveCommand != null)
                    {
                        _moveCommand.Undo();

                        var group = false;
                        if (_copyCommand != null)
                        {
                            group = true;
                            _copyCommand.Undo();
                            ExecuteCommand(_copyCommand);
                            _copyCommand = null;
                        }

                        ExecuteCommand(_moveCommand, group);
                        _moveCommand = null;

                        SelectTiles(_moveTiles);
                    }

                    break;
                }

                case MoveState.Selecting:
                    FitSelection();
                    break;
            }

            if(_moveCommand != null)
            {
                _moveCommand.Undo();
                _moveCommand = null;
            }

            _moveTiles = null;
            _moveDragState = MoveState.None;
            _moveAnchor = Cell.invalid;
            _moveLayerMask = 0;

            UpdateCursor();
        }

        private CursorType OnMoveGetCursor(Cell cell)
        {
            if (_moveDragState != MoveState.Selecting && IsSelected(_cursorWorld))
                return CursorType.ArrowWithMove;

            return CursorType.Arrow;
        }

        /// <summary>
        /// Handle keyboard keys for move too
        /// </summary>
        /// <param name="keyCode">Key</param>
        private void OnMoveKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Delete:
                    var tiles = GetSelectedTiles();
                    if (null == tiles || tiles.Length == 0)
                        return;

                    var command = new Editor.Commands.GroupCommand();
                    foreach(var tile in tiles)
                        Erase(tile, command);

                    ExecuteCommand(command);
                    break;
            }
        }

        private Tile[] CloneTiles (Tile[] tiles)
        {
            var cloned = new Tile[tiles.Length];
            for (int i = 0; i < tiles.Length; i++)
            {
                cloned[i] = puzzle.InstantiateTile(DatabaseManager.GetTile(tiles[i].guid), Cell.invalid);
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                var sourcePorts = tiles[i].GetPorts();
                for (int j=0; j < sourcePorts.Length; j++)
                {
                    var sourcePort = sourcePorts[j];
                    if (sourcePort.flow != PortFlow.Output)
                        continue;

                    foreach(var sourceWire in sourcePort.wires)
                    {
                        var fromPortTileIndex = 0;
                        for(; fromPortTileIndex < tiles.Length && tiles[fromPortTileIndex] != sourceWire.from.tile; fromPortTileIndex++);
                        if (fromPortTileIndex >= tiles.Length)
                            continue;

                        var toPortTileIndex = 0;
                        for (; toPortTileIndex < tiles.Length && tiles[toPortTileIndex] != sourceWire.to.tile; toPortTileIndex++);
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
                    if(property.type != TilePropertyType.Port && property.editable.serialized)
                        property.SetValue(cloned[i], property.GetValue(tiles[i]));
            }

            return cloned;
        }
    }
}
