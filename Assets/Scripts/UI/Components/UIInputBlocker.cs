using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Puzzled
{
    public class UIInputBlocker : MonoBehaviour, IPointerClickHandler, KeyboardManager.IKeyboardHandler
    {
        public UnityEvent onCancel = new UnityEvent();

        private void OnEnable()
        {
            KeyboardManager.Push(this);
        }

        private void OnDisable()
        {
            KeyboardManager.Pop();
        }

        private void Cancel() => onCancel?.Invoke();

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                Cancel();
        }

        void KeyboardManager.IKeyboardHandler.OnKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Escape:
                    Cancel();
                    break;
            }
        }
    }
}
