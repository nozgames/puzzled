using System;
using NoZ;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Puzzled.UI
{
    public class UIManager : MonoBehaviour
    {
        [Serializable]
        private struct CursorInfo
        {
            public Texture2D cursor;
            public Vector2 hotspot;
        }

        [Header("General")]
        [SerializeField] private UIScreen startScreen = null;
        [SerializeField] private Transform popups = null;
        [SerializeField] private Transform popupCentered = null;
        [SerializeField] private GameObject _loading = null;
        [SerializeField] private UITooltipPopup _tooltip = null;

        [Header("HUD")]
        [SerializeField] private UIScreen _hud = null;
        [SerializeField] private GameObject _hudPlayerItem = null;
        [SerializeField] private RawImage _hudPlayerItemIcon = null;

        [Header("Screens")]
        [SerializeField] private UIScreen _mainScreen = null;
        [SerializeField] private UIScreen _pauseScreen = null;
        [SerializeField] private UIScreen _createScreen = null;
        [SerializeField] private UIScreen _playScreen = null;
        [SerializeField] private UIPlayWorldScreen _playWorldScreen = null;
        [SerializeField] private UIEditWorldScreen _editWorldScreen = null;
        [SerializeField] private UIEditWorldPropertiesScreen _editWorldPropertiesScreen = null;
        [SerializeField] private UIWorldTransitionScreen _worldTransitionScreen = null;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference upAction = null;
        [SerializeField] private InputActionReference downAction = null;
        [SerializeField] private InputActionReference leftAction = null;
        [SerializeField] private InputActionReference rightAction = null;
        [SerializeField] private InputActionReference confirmAction = null;
        [SerializeField] private InputActionReference cancelAction = null;
        [SerializeField] private InputActionReference menuAction = null;
        [SerializeField] private InputActionReference optionAction = null;

        [Header("Popups")]
        [SerializeField] private UINamePopup _namePopup = null;
        [SerializeField] private UIConfirmPopup _confirmPopup = null;

        [Header("Cursors")]
        [SerializeField] private CursorInfo[] _cursors = null;

        private CursorType _cursor = CursorType.Arrow;

        public static CursorType cursor {
            get => _instance._cursor;
            set {
                if (value == _instance._cursor)
                    return;

                _instance._cursor = value;
                var info = _instance._cursors[(int)value];
                Cursor.SetCursor(info.cursor, info.hotspot, CursorMode.Auto);
            }
        }

        public static UIManager _instance { get; private set; }

        public UIScreen activeScreen { get; private set; }

        public static bool loading {
            get => _instance._loading.activeSelf;
            set => _instance._loading.SetActive(value);
        }

        private void Awake()
        {
            _instance = this;

            upAction.action.started += OnUpAction;
            downAction.action.started += OnDownAction;
            leftAction.action.started += OnLeftAction;
            rightAction.action.started += OnRightAction;
            confirmAction.action.started += OnConfirmAction;
            cancelAction.action.started += OnCancelAction;
            menuAction.action.started += OnMenuAction;
            optionAction.action.started += OnOptionAction;

            upAction.action.Enable();
            downAction.action.Enable();
            leftAction.action.Enable();
            rightAction.action.Enable();
            confirmAction.action.Enable();
            cancelAction.action.Enable();
            menuAction.action.Enable();
            optionAction.action.Enable();
        }

        public static void Initialize ()
        {
            foreach (var screen in _instance.GetComponentsInChildren<UIScreen>(true))
                screen.gameObject.SetActive(false);

            _instance.startScreen.gameObject.SetActive(true);
            _instance.activeScreen = _instance.startScreen;

            _instance._cursor = CursorType.ArrowWithMinus;
            cursor = CursorType.Arrow;

            loading = true;
        }

        public static void Shutdown ()
        {
        }

        private void OnDestroy()
        {
            _instance = null;

            upAction.action.Disable();
            downAction.action.Disable();
            leftAction.action.Disable();
            rightAction.action.Disable();
            confirmAction.action.Disable();
            cancelAction.action.Disable();
            menuAction.action.Disable();
        }

        private static void SetActiveScreen (UIScreen screen)
        {
            if(_instance.activeScreen != null)
            {
                _instance.activeScreen.gameObject.SetActive(false);
                _instance.activeScreen = null;
            }

            _instance.activeScreen = screen;
            if (_instance.activeScreen != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                _instance.activeScreen.gameObject.SetActive(true);
            }
        }

        public static void TogglePauseScreen()
        {
            if ((_instance.activeScreen == _instance._pauseScreen) && _instance._pauseScreen.gameObject.activeInHierarchy)
                HideMenu();
            else
                SetActiveScreen(_instance._pauseScreen);
        }

        public static void ShowPauseScreen() => SetActiveScreen(_instance._pauseScreen);

        public static void ShowMainScreen() => SetActiveScreen(_instance._mainScreen);

        public static void ShowCreateScreen() => SetActiveScreen(_instance._createScreen);

        public static void ShowPlayScreen() => SetActiveScreen(_instance._playScreen);

        public static void EnterPlayWorldScreen(World world, bool isDebugging = false)
        {
            _instance._playWorldScreen.isDebugging = isDebugging;

            if (world != null)
            {
                _instance._playWorldScreen.world = world;

                var transition = world.transitionIn;
                if(transition != null)
                {
                    ShowWorldTransitionScreen(transition, () => {
                        SetActiveScreen(_instance._playWorldScreen);
                    });
                }
                else
                    SetActiveScreen(_instance._playWorldScreen);
            }
        }

        public static void ReturnToPlayWorldScreen()
        {
            Debug.Assert(_instance._playWorldScreen.world != null);

            SetActiveScreen(_instance._playWorldScreen);
        }

        public static void EnterEditWorldScreen(World world)
        {
            if (world != null)
                _instance._editWorldScreen.world = world;

            SetActiveScreen(_instance._editWorldScreen);
        }

        public static void ReturnToEditWorldScreen()
        {
            Debug.Assert(_instance._editWorldScreen.world != null);

            SetActiveScreen(_instance._editWorldScreen);
        }

        public static void ShowEditWorldPropertiesScreen(World _world)
        {
            _instance._editWorldPropertiesScreen.world = _world;
            SetActiveScreen(_instance._editWorldPropertiesScreen);
        }

        public static void ShowWorldTransitionScreen(World.Transition transition, Action callback)
        {
            _instance._worldTransitionScreen.transition = transition;
            _instance._worldTransitionScreen.callback = callback;
            SetActiveScreen(_instance._worldTransitionScreen);
        }

        public static void HideMenu ()
        {
            if (null == _instance.activeScreen)
                return;

            _instance.activeScreen.gameObject.SetActive(false);
            _instance.activeScreen = null;
        }

        public static UIPopup ShowPopup(UIPopup popupPrefab, Action doneCallback = null)
        {
            _instance.popups.gameObject.SetActive(true);
            UIPopup popup = Instantiate(popupPrefab, _instance.popupCentered).GetComponent<UIPopup>();
            popup.doneCallback = doneCallback;

            return popup;
        }

        public static void ClosePopup()
        {
            _instance.popups.gameObject.SetActive(false);
            _instance.popupCentered.DetachAndDestroyChildren();
        }

        public static void ShowTooltip(Vector3 position, string text, TooltipDirection direction)
        {
            var screen = CameraManager.WorldToScreen(position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _instance.GetComponent<RectTransform>(),
                screen,
                null,
                out var local);
            _instance._tooltip.Show(new Rect(local, Vector2.zero), text, direction);
        }

        public static void HideTooltip()
        {
            _instance._tooltip.gameObject.SetActive(false);
        }

        public static void ShowHud (bool show=true)
        {
            _instance._hud.gameObject.SetActive(show);
        }

        public static void SetPlayerItem (Tile tile)
        {
            if(tile == null)
            {
                Tween.Scale(1, 0).Key("Item").Duration(0.25f).EaseInOutBack().AutoDeactivate().Start(_instance._hudPlayerItem.gameObject);
            }
            else
            {
                _instance._hudPlayerItem.gameObject.SetActive(true);
                Tween.Scale(0, 1).Key("Item").Duration(0.25f).EaseInOutElastic(1, 3).Start(_instance._hudPlayerItem.gameObject);
                _instance._hudPlayerItemIcon.texture = DatabaseManager.GetPreview(tile.guid);
            }
        }

        public static void ShowNamePopup (string value = null, string title = null, string commit = null, string placeholder = null, Func<string,string> onCommit = null, Action onCancel = null)
        {
            _instance._namePopup.Show(value, title, commit, placeholder, onCommit, onCancel);
        }

        public static void ShowConfirmPopup(string message = null, string title = null, string confirm = null, Action onConfirm = null, Action onCancel = null)
        {
            _instance._confirmPopup.Show(message, title, confirm, onConfirm, onCancel);
        }

        private void OnUpAction(InputAction.CallbackContext obj)
        {
            if ((activeScreen == null) || !activeScreen.gameObject.activeInHierarchy)
                return;

            activeScreen.HandleUpInput();
        }

        private void OnDownAction(InputAction.CallbackContext obj)
        {
            if ((activeScreen == null) || !activeScreen.gameObject.activeInHierarchy)
                return;

            activeScreen.HandleDownInput();
        }

        private void OnLeftAction(InputAction.CallbackContext obj)
        {
            if ((activeScreen == null) || !activeScreen.gameObject.activeInHierarchy)
                return;

            activeScreen.HandleLeftInput();
        }

        private void OnRightAction(InputAction.CallbackContext obj)
        {
            if ((activeScreen == null) || !activeScreen.gameObject.activeInHierarchy)
                return;

            activeScreen.HandleRightInput();
        }

        private void OnConfirmAction(InputAction.CallbackContext obj)
        {
            if ((activeScreen == null) || !activeScreen.gameObject.activeInHierarchy)
                return;

            activeScreen.HandleConfirmInput();
        }

        private void OnCancelAction(InputAction.CallbackContext obj)
        {
            if ((activeScreen == null) || !activeScreen.gameObject.activeInHierarchy)
                return;

            activeScreen.HandleCancelInput();
        }

        private void OnMenuAction(InputAction.CallbackContext obj)
        {
            if ((activeScreen != null) && activeScreen.gameObject.activeInHierarchy)
            {
                activeScreen.HandleMenuInput();
                return;
            }

            if (_hud.gameObject.activeInHierarchy)
                _hud.HandleMenuInput();
        }

        private void OnOptionAction(InputAction.CallbackContext obj)
        {
            if ((activeScreen != null) && activeScreen.gameObject.activeInHierarchy)
            {
                activeScreen.HandleOptionInput();
                return;
            }

            if (_hud.gameObject.activeInHierarchy)
                _hud.HandleOptionInput();
        }
    }
}
