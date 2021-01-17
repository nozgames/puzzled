using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Puzzled
{
    class KeyboardManager : MonoBehaviour
    {
        public interface IKeyboardHandler
        {
            void OnKey(KeyCode keyCode);
            void OnModifiersChanged(bool shift, bool ctrl, bool alt);
        }

        [Header("Keys")]
        [SerializeField] private InputAction keyEscape = null;
        [SerializeField] private InputAction keyDelete = null;
        [SerializeField] private InputAction keyB = null;
        [SerializeField] private InputAction keyD = null;
        [SerializeField] private InputAction keyE = null;
        [SerializeField] private InputAction keyF = null;
        [SerializeField] private InputAction keyV = null;
        [SerializeField] private InputAction keyW = null;
        [SerializeField] private InputAction keyY = null;
        [SerializeField] private InputAction keyZ = null;
        [SerializeField] private InputAction key1 = null;
        [SerializeField] private InputAction key2 = null;
        [SerializeField] private InputAction key3 = null;
        [SerializeField] private InputAction key4 = null;

        [Header("Modifiers")]
        [SerializeField] private InputAction keyShift = null;
        [SerializeField] private InputAction keyAlt = null;
        [SerializeField] private InputAction keyCtrl = null;

        private static KeyboardManager _instance = null;

        private bool shift = false;
        private bool ctrl = false;
        private bool alt = false;
        private Stack<IKeyboardHandler> _handlers = new Stack<IKeyboardHandler>();

        public static bool isShiftPressed {
            get => _instance.shift;
            private set {
                _instance.shift = value;
                _instance.UpdateModifiers();
            }
        }

        public static bool isCtrlPressed {
            get => _instance.ctrl;
            private set {
                _instance.ctrl = value;
                _instance.UpdateModifiers();
            }
        }

        public static bool isAltPressed {
            get => _instance.alt;
            private set {
                _instance.alt = value;
                _instance.UpdateModifiers();
            }
        }

        private void Awake()
        {
            keyEscape.performed += (ctx) => SendKey(KeyCode.Escape);
            keyDelete.performed += (ctx) => SendKey(KeyCode.Delete);
            key1.performed += (ctx) => SendKey(KeyCode.Alpha1);
            key2.performed += (ctx) => SendKey(KeyCode.Alpha2);
            key3.performed += (ctx) => SendKey(KeyCode.Alpha3);
            key4.performed += (ctx) => SendKey(KeyCode.Alpha4);
            keyW.performed += (ctx) => SendKey(KeyCode.W);
            keyE.performed += (ctx) => SendKey(KeyCode.E);
            keyF.performed += (ctx) => SendKey(KeyCode.F);
            keyB.performed += (ctx) => SendKey(KeyCode.B);
            keyD.performed += (ctx) => SendKey(KeyCode.D);
            keyV.performed += (ctx) => SendKey(KeyCode.V);
            keyZ.performed += (ctx) => SendKey(KeyCode.Z);
            keyY.performed += (ctx) => SendKey(KeyCode.Y);
            keyShift.started += (ctx) => isShiftPressed = true;
            keyShift.canceled += (ctx) => isShiftPressed = false;
            keyAlt.started += (ctx) => isAltPressed = true;
            keyAlt.canceled += (ctx) => isAltPressed = false;
            keyCtrl.started += (ctx) => isCtrlPressed = true;
            keyCtrl.canceled += (ctx) => isCtrlPressed = false;
        }

        private void OnEnable()
        {
            _instance = this;

            keyEscape.Enable();
            keyDelete.Enable();
            key1.Enable();
            key2.Enable();
            key3.Enable();
            key4.Enable();
            keyW.Enable();
            keyE.Enable();
            keyF.Enable();
            keyB.Enable();
            keyD.Enable();
            keyV.Enable();
            keyY.Enable();
            keyZ.Enable();
            keyShift.Enable();
            keyAlt.Enable();
            keyCtrl.Enable();
        }

        private void OnDisable()
        {
            keyEscape.Disable();
            keyDelete.Disable();
            keyY.Disable();
            keyZ.Disable();
            keyShift.Disable();
            keyAlt.Disable();
            keyCtrl.Disable();

            _instance = null;
        }

        public static void Push (IKeyboardHandler handler)
        {
            _instance._handlers.Push(handler);
        }

        public static void Pop ()
        {
            _instance._handlers.Pop();
        }

        private IKeyboardHandler GetHandler()
        {
            if (_instance._handlers.Count == 0)
                return null;

            // All input when a keyboard control is active is ignored
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
                return null;

            return _instance._handlers.Peek();
        }

        private void SendKey(KeyCode keyCode)
        {
            if (null != EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
                return;

            GetHandler()?.OnKey(keyCode);
        }

        private void UpdateModifiers() => GetHandler()?.OnModifiersChanged(_instance.shift, _instance.ctrl, _instance.alt);
    }
}
