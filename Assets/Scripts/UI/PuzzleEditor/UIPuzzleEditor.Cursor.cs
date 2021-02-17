using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private const float CursorEdgeBias = 0.25f;

        [Header("Cursor")]
        [SerializeField] private InputActionReference _pointerAction = null;

        private Cell _cursorCell = Cell.invalid;
        private Vector3 _cursorWorld = Vector3.zero;
        private Func<Cell, CursorType> _getCursor = null;

        private void InitializeCursor()
        {
            canvas.onEnter = OnPointerEnterCanvas;
            canvas.onExit = OnPointerExitCanvas;
            _pointerAction.action.performed -= OnPointerMoved;
            _pointerAction.action.performed += OnPointerMoved;
        }

        private void OnPointerExitCanvas()
        {
            _cursorGizmo.gameObject.SetActive(false);
            UIManager.cursor = CursorType.Arrow;
            _cursorCell = Cell.invalid;
        }

        private void OnPointerEnterCanvas()
        {            
        }

        private void OnPointerMoved(InputAction.CallbackContext ctx) => UpdateCursor(true);

        private void UpdateCursor(bool updatePosition = false)
        {
            if (popups.activeSelf || playing)
            {
                UIManager.cursor = CursorType.Arrow;
                return;
            }

            if (updatePosition && canvas.isMouseOver)
            {
                var position = _pointerAction.action.ReadValue<Vector2>();
                var cell = canvas.CanvasToCell(position);
                if (cell == Cell.invalid)
                    return;

                _cursorWorld = canvas.CanvasToWorld(position);

                // Check for cursor continuity on the same edge
                var old = _cursorCell.NormalizeEdge();
                if (old.edge == Cell.Edge.North || old.edge == Cell.Edge.East)
                {
                    var bounds = puzzle.grid.CellToWorldBounds(old);
                    bounds.Expand(CursorEdgeBias);
                    if (old.edge == Cell.Edge.North && _cursorWorld.z >= bounds.min.z && _cursorWorld.z <= bounds.max.z)
                        cell = new Cell(cell.x, cell.y, cell.y == old.y ? Cell.Edge.North : Cell.Edge.South);
                    else if (old.edge == Cell.Edge.East && _cursorWorld.x >= bounds.min.x && _cursorWorld.x <= bounds.max.x)
                        cell = new Cell(cell.x, cell.y, cell.x == old.x ? Cell.Edge.East : Cell.Edge.West);
                }

                _cursorCell = cell;
            }

            if (_cursorCell == Cell.invalid)
                return;

            var renderCell = _cursorCell;

            if (mode == Mode.Move && _moveDragState == MoveState.Moving && selectedTile != null && _moveAnchor.isEdge)
            {
                var cellCenter = puzzle.grid.CellToWorld(_cursorCell);
                var cellCenterOffset = _cursorWorld - cellCenter;

                if (_moveAnchor.NormalizeEdge().edge == Cell.Edge.North)
                    renderCell.edge = (cellCenterOffset.z > 0 ? Cell.Edge.North : Cell.Edge.South);
                else
                    renderCell.edge = (cellCenterOffset.x > 0 ? Cell.Edge.East : Cell.Edge.West);
            }
            else if (mode == Mode.Move && _moveDragState == MoveState.Moving)
            {
                renderCell.edge = Cell.Edge.None;
            }
            else if (mode == Mode.Draw && !KeyboardManager.isAltPressed)
            {
                if (_tilePalette.selected == null || !TileGrid.IsEdgeLayer(_tilePalette.selected.layer))
                    renderCell.edge = Cell.Edge.None;
            }
            else
            {
                var cellCenter = puzzle.grid.CellToWorld(_cursorCell);
                var cellCenterOffset = _cursorWorld - cellCenter;
                if (Mathf.Abs(cellCenterOffset.x) < 0.25f && Mathf.Abs(cellCenterOffset.z) < 0.25f)
                    renderCell.edge = Cell.Edge.None;
                else if (renderCell.edge != Cell.Edge.None && puzzle.grid.CellToTile(renderCell) == null)
                    renderCell.edge = Cell.Edge.None;
            }

            _cursorCell = renderCell;

            UIManager.cursor = _getCursor?.Invoke(renderCell) ?? CursorType.Arrow;

            _cursorGizmo.gameObject.SetActive(canvas.isMouseOver);

            var cursorBounds = puzzle.grid.CellToWorldBounds(renderCell);
            _cursorGizmo.min = cursorBounds.min;
            _cursorGizmo.max = cursorBounds.max;
        }
    }
}
