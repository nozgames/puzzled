using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        [Header("Cursor")]
        [SerializeField] private InputActionReference _pointerAction = null;

        private Cell _cursorCell = Cell.invalid;
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

        private void OnPointerMoved(InputAction.CallbackContext ctx)
        {
            if (!canvas.isMouseOver)
                return;

            var cell = canvas.CanvasToCell(ctx.ReadValue<Vector2>());
            if (cell == _cursorCell)
                return;

            if (cell == Cell.invalid)
                return;

            _cursorCell = cell;
            UpdateCursor();
        }

        private void UpdateCursor(bool updatePosition = false)
        {
            if (popups.activeSelf || playing)
            {
                UIManager.cursor = CursorType.Arrow;
                return;
            }

            if (updatePosition && canvas.isMouseOver)
            {
                var cell = canvas.CanvasToCell(_pointerAction.action.ReadValue<Vector2>());
                if (cell == Cell.invalid)
                    return;

                _cursorCell = cell;
            }

            UIManager.cursor = _getCursor?.Invoke(_cursorCell) ?? CursorType.Arrow;

            _cursorGizmo.gameObject.SetActive(canvas.isMouseOver);
            _cursorGizmo.min = puzzle.grid.CellToWorld(_cursorCell) - new Vector3(0.5f, 0.0f, 0.5f);
            _cursorGizmo.max = puzzle.grid.CellToWorld(_cursorCell) + new Vector3(0.5f, 0.0f, 0.5f);
        }
    }
}
