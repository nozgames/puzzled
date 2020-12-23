using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor 
    {
        private enum MoveDragState
        {
            None,
            PreSelect,
            PreMove,
            Moving,
            Selecting
        }

        private Cell _selectionDragStart;
        private Tile[] _movingTiles;
        private Cell _moveDragMin;
        private Cell _moveDragMax;
        private MoveDragState _moveDragState = MoveDragState.None;

        private void EnableMoveTool ()
        {
            moveToolOptions.SetActive(true);

            canvas.onLButtonDown = OnMoveToolLButtonDown;
            canvas.onLButtonUp = OnMoveToolLButtonUp;
            canvas.onLButtonDragBegin = onMoveToolDragBegin;
            canvas.onLButtonDrag = onMoveToolDrag;
            canvas.onLButtonDragEnd = onMoveToolDragEnd;

            _onKey = OnMoveKey;
            _moveDragState = MoveDragState.None;

            _getCursor = OnMoveGetCursor;
        }

        private void DisableMoveTool()
        {
            moveToolOptions.SetActive(false);
        }

        private void OnMoveToolLButtonDown(Vector2 position)
        {
            var cell = canvas.CanvasToCell(position);

            _selectionDragStart = cell;
            if (isCellInSelection(cell))
                _moveDragState = MoveDragState.PreMove;
            else
            {
                _moveDragState = MoveDragState.PreSelect;
                SetSelectionRect(_selectionDragStart, _selectionDragStart);
            }
        }

        private void onMoveToolDragBegin(Vector2 position)
        {
            if (_moveDragState == MoveDragState.PreMove)
                _moveDragState = MoveDragState.Moving;
            else if (_moveDragState == MoveDragState.PreSelect)
                _moveDragState = MoveDragState.Selecting;
            else
                _moveDragState = MoveDragState.None;

            if (_moveDragState == MoveDragState.Moving)
            {
                // Get all tiles in the selection area that match the enbled layers
                _movingTiles = puzzle.grid.GetLinkedTiles(_selectionMin, _selectionMax).Where(t => layerToggles[(int)t.info.layer].isOn).ToArray();
                _moveDragMin = _selectionMin;
                _moveDragMax = _selectionMax;
            }

            UpdateCursor();
        }

        private void onMoveToolDragEnd(Vector2 position)
        {

        }


        private void onMoveToolDrag(Vector2 position, Vector2 delta)
        {
            if (_moveDragState == MoveDragState.Selecting)
            {
                SetSelectionRect(_selectionDragStart, canvas.CanvasToCell(position));
                return;
            }
            
            if(_moveDragState == MoveDragState.Moving)
            {
                var offset = canvas.CanvasToCell(position) - _selectionDragStart;
                var wdelta = canvas.CanvasToWorld(position + delta) - canvas.CanvasToWorld(position);
                foreach (var tile in _movingTiles)
                    tile.transform.position = puzzle.grid.CellToWorld(tile.cell + offset);

                SetSelectionRect(_moveDragMin + offset, _moveDragMax + offset);

                UpdateCursor();
            }
        }

        private void OnMoveToolLButtonUp (Vector2 position)
        {
            if ((_moveDragState == MoveDragState.PreMove || _moveDragState == MoveDragState.PreSelect) && GetTile(_selectionDragStart) != null)
                SetSelectionRect(_selectionDragStart, _selectionDragStart);
            else if (_moveDragState == MoveDragState.Selecting)
                FitSelectionRect();
            else if (_moveDragState == MoveDragState.Moving && _movingTiles != null)
            { 
                if (!CanDrop())
                {
                    // Return the tiles back to where they were since it failed
                    foreach (var tile in _movingTiles)
                        tile.transform.position = puzzle.grid.CellToWorld(tile.cell);

                    SetSelectionRect(_moveDragMin, _moveDragMax);
                } 
                else
                    ExecuteCommand(new Editor.Commands.TileMoveCommand(_movingTiles, _moveDragMin, _selectionMin, _selectionSize));
            }
            else
                selectionRect.gameObject.SetActive(false);

            _movingTiles = null;
            _moveDragState = MoveDragState.None;

            UpdateCursor();
        }

        /// <summary>
        /// Fit the selection rect to the visible tiles within it
        /// </summary>
        private void FitSelectionRect()
        {
            if (!selectionRect.gameObject.activeSelf)
                return;

            var tiles = puzzle.grid.GetLinkedTiles(_selectionMin, _selectionMax).Where(t => layerToggles[(int)t.info.layer].isOn).ToArray();
            if (tiles.Length == 0)
                selectionRect.gameObject.SetActive(false);
            else
            {
                _selectionMin = _selectionMax = tiles[0].cell;
                foreach (var tile in tiles)
                {
                    _selectionMin = Cell.Min(_selectionMin, tile.cell);
                    _selectionMax = Cell.Max(_selectionMax, tile.cell);
                }
                SetSelectionRect(_selectionMin, _selectionMax);
            }
        }

        private bool isCellInSelection(Cell cell) =>
            selectionRect.gameObject.activeSelf && (cell.x >= _selectionMin.x && cell.x <= _selectionMax.x && cell.y >= _selectionMin.y && cell.y <= _selectionMax.y);

        private CursorType OnMoveGetCursor(Cell cell)
        {
            if (_moveDragState == MoveDragState.PreMove)
                return CursorType.ArrowWithMove;

            if(_moveDragState == MoveDragState.Moving)
                return CanDrop() ? CursorType.ArrowWithMove : CursorType.ArrowWithNot;

            if (_moveDragState == MoveDragState.Selecting || _moveDragState == MoveDragState.PreSelect)
                return CursorType.Arrow;

            if (isCellInSelection(cell))
                return CursorType.ArrowWithMove;

            return CursorType.Arrow;
        }

        private bool CanDrop ()
        {
            var offset = _selectionMin - _moveDragMin;
            foreach(var tile in _movingTiles)
            {
                var cell = tile.cell + offset;

                // If the tile is within the original selection rect then we know its ok to drop 
                if (cell.x >= _moveDragMin.x && cell.x <= _moveDragMax.x && cell.y >= _moveDragMin.y && cell.y <= _moveDragMax.y)
                    continue;

                // Look for any tile on the same layer in the new cell
                if (null != puzzle.grid.CellToTile(cell, tile.info.layer))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Return and array of all selected tiles
        /// </summary>
        private Tile[] GetSelectedTiles() =>
            selectionRect.gameObject.activeSelf ?
                puzzle.grid.GetLinkedTiles(_selectionMin, _selectionMax).Where(t => layerToggles[(int)t.info.layer].isOn).ToArray() :
                null;

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
                    if (null == tiles)
                        return;

                    var combine = false;
                    foreach(var tile in tiles)
                    {
                        ExecuteCommand(new Editor.Commands.TileDestroyCommand(tile), combine);
                        combine = true;
                    }
                    break;
            }
        }
    }
}
