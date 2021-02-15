using System.Linq;
using UnityEngine;

namespace Puzzled
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

        /// <summary>
        /// Anchor cell for moving tiles
        /// </summary>
        private Cell _moveAnchor;

        /// <summary>
        /// Current offset from the anchor
        /// </summary>
        private Cell _moveOffset;

        /// <summary>
        /// Mask of layers participating in the move
        /// </summary>
        private uint _moveLayerMask;

        /// <summary>
        /// Last move command issued
        /// </summary>
        private Editor.Commands.Command _moveCommand;

        /// <summary>
        /// Current move tool state
        /// </summary>
        private MoveState _moveDragState = MoveState.None;

        private void EnableMoveTool ()
        {
            moveToolOptions.SetActive(true);

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

            moveToolOptions.SetActive(false);
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
                _moveDragState = MoveState.Moving;
            } 
            // If dragging from a selected cell then start moving
            else if(IsSelected(_cursorCell))
            {
                _moveAnchor = _cursorCell;
                _moveTiles = GetSelectedTiles();
                _moveDragState = MoveState.Moving;
                _moveLayerMask = GetVisibleLayerMask();
            } 
            // Start dragging a new selection
            else
            {
                _moveAnchor.edge = Cell.Edge.None;
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
        private Editor.Commands.Command CreateMoveCommand (Tile[] tiles, Cell offset, uint layerMask)
        {
            var command = new Editor.Commands.GroupCommand();

            // Unlink all tiles being moved
            foreach (var tile in tiles)
                command.Add(new Editor.Commands.TileMoveCommand(tile, Cell.invalid));

            // Destroy any tiles in the target that overlap the tiles being moved
            foreach (var tile in tiles)
            {
                var existing = puzzle.grid.CellToTile(tile.cell + offset, tile.layer);
                if (null == existing || tiles.Contains(existing))
                    continue;

                Erase(existing, command);
            }
            
            // Link the new tiles in the moved position
            foreach (var tile in tiles)
                command.Add(new Editor.Commands.TileMoveCommand(tile, tile.cell + offset, Cell.invalid));

            return command;
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
                    var offset = Cell.zero;
                    if (selectedTile != null)
                        offset = _cursorCell.NormalizeEdge() - _moveAnchor.NormalizeEdge();
                    else
                        offset = (new Cell(_cursorCell, Cell.Edge.None) - _moveAnchor);

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
                    _moveCommand = CreateMoveCommand(_moveTiles, _moveOffset, _moveLayerMask);
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
                    if (selectedTile != null && selectedTile.cell == _cursorCell)
                        SelectTile(GetNextTile(selectedTile));
                    else
                        SelectTile(_cursorCell);

                    break;
                }
                
                case MoveState.Moving:
                {
                    if (_moveTiles == null)
                        break;

                    if (_moveCommand != null)
                    {
                        _moveCommand.Undo();
                        ExecuteCommand(_moveCommand);
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
            if (_moveDragState != MoveState.Selecting && IsSelected(cell))
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
    }
}
