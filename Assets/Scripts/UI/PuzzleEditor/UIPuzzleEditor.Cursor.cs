using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Puzzled.UI;

namespace Puzzled.Editor
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
            _canvas.onEnter = OnPointerEnterCanvas;
            _canvas.onExit = OnPointerExitCanvas;
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

            // When drawing we want to lock the cursor to the coordinate system of the tile being drawn.
            var coordinateSystem = CellCoordinateSystem.Edge;
            if (mode == Mode.Draw && _tilePalette.selected != null)
                coordinateSystem = _puzzle.grid.LayerToCoordinateSystem(_tilePalette.selected.layer);
            else if (mode == Mode.Erase && _eraseLayerOnly)
                coordinateSystem = _puzzle.grid.LayerToCoordinateSystem(_eraseLayer);

            if (updatePosition && _canvas.isMouseOver)
            {
                var position = _pointerAction.action.ReadValue<Vector2>();
                var cell = _canvas.CanvasToCell(position, coordinateSystem);
                if (cell == Cell.invalid)
                    return;

                _cursorWorld = _canvas.CanvasToWorld(position);

                // When moving a single tile around that is on an edge lock the edge to that edge
                if (mode == Mode.Move && _moveDragState == MoveState.Moving && selectedTile != null && selectedTile.cell.edge != CellEdge.None)
                    cell = new Cell(selectedTile.cell.system, cell.x, cell.y, selectedTile.cell.edge);
                // When the cursor was an edge and will be an edge try to maintain some contiunity beween the edges
                else if (cell.edge != CellEdge.None && _cursorCell.edge != CellEdge.None && _cursorCell.edge != cell.edge)
                {
                    // If the cursor is still within the previous cell then dont change.  This will
                    // help prevent oscillation between vertical and horizontal edges on the corners.
                    if (_puzzle.grid.CellToWorldBounds(_cursorCell).Contains(_cursorWorld))
                        cell = _cursorCell;
                    // Otherwise check to see if the cursor is over a parallel edge to the previous cell 
                    // and if so use that cell.  This will help transitioning from one cell to the other
                    // on the same edge and prevent it from oscillating between vertical and horizontal edges.
                    else
                    {
                        var parallel = Cell.GetParallelEdge(_cursorCell, CellEdgeSide.Min);
                        if (_puzzle.grid.CellToWorldBounds(parallel).Contains(_cursorWorld))
                            cell = parallel;
                        else
                        {
                            parallel = Cell.GetParallelEdge(_cursorCell, CellEdgeSide.Max);
                            if (_puzzle.grid.CellToWorldBounds(parallel).Contains(_cursorWorld))
                                cell = parallel;
                        }
                    }
                }

                if (mode != Mode.Draw && 
                    !(mode == Mode.Erase && _eraseLayerOnly) &&
                    !(mode == Mode.Move && _moveDragState == MoveState.Moving && selectedTile != null))
                {
                    Debug.Assert(cell.edge != CellEdge.None);

                    if (!_puzzle.grid.CellContainsWorldPoint(cell,_cursorWorld) || null == GetTopMostTile(cell))
                    {
                        var sharedEdgeCell = cell.ConvertTo(CellCoordinateSystem.SharedEdge);
                        if (!_puzzle.grid.CellContainsWorldPoint(sharedEdgeCell,_cursorWorld) || null == _puzzle.grid.CellToTile(sharedEdgeCell))
                            cell = cell.ConvertTo(CellCoordinateSystem.Grid);
                        else
                            cell = sharedEdgeCell;
                    }
                }

                _cursorCell = cell;
            }

            if (_cursorCell == Cell.invalid)
                return;

            var renderCell = _cursorCell;



#if false
            if (mode == Mode.Move && _moveDragState == MoveState.Moving && selectedTile != null && _moveAnchor.isEdge)
            {
                var cellCenter = puzzle.grid.CellToWorld(_cursorCell);
                var cellCenterOffset = _cursorWorld - cellCenter;

                if (_moveAnchor.NormalizeEdge().edge == CellEdge.North)
                    renderCell.edge = (cellCenterOffset.z > 0 ? CellEdge.North : CellEdge.South);
                else
                    renderCell.edge = (cellCenterOffset.x > 0 ? CellEdge.East : CellEdge.West);
            }
            else if (mode == Mode.Move && _moveDragState == MoveState.Moving)
            {
                renderCell.edge = CellEdge.None;
            }
            else if (mode == Mode.Draw && !KeyboardManager.isAltPressed)
            {
                if (_tilePalette.selected == null || !TileGrid.IsEdgeLayer(_tilePalette.selected.layer))
                    renderCell.edge = CellEdge.None;
            }
            else
            {
                var cellCenter = puzzle.grid.CellToWorld(_cursorCell);
                var cellCenterOffset = _cursorWorld - cellCenter;
                if (Mathf.Abs(cellCenterOffset.x) < 0.25f && Mathf.Abs(cellCenterOffset.z) < 0.25f)
                    renderCell.edge = CellEdge.None;
                else if (renderCell.edge != CellEdge.None && puzzle.grid.CellToTile(renderCell) == null)
                    renderCell.edge = CellEdge.None;
            }
#endif
            _cursorCell = renderCell;

            UIManager.cursor = _getCursor?.Invoke(renderCell) ?? CursorType.Arrow;

            _cursorGizmo.gameObject.SetActive(_canvas.isMouseOver);

            var cursorBounds = puzzle.grid.CellToWorldBounds(renderCell);
            _cursorGizmo.min = cursorBounds.min;
            _cursorGizmo.max = cursorBounds.max;
        }
    }
}
