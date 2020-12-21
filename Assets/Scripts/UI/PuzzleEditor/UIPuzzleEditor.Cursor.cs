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
            
            _cursorCell = cell;
            UpdateCursor();
        }

        private void UpdateCursor(bool updatePosition = false)
        {
            if(updatePosition && canvas.isMouseOver)
                _cursorCell = canvas.CanvasToCell(_pointerAction.action.ReadValue<Vector2>());                

            UIManager.cursor = _getCursor?.Invoke(_cursorCell) ?? CursorType.Arrow;
        }
    }
}
