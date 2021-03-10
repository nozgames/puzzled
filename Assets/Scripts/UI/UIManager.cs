﻿using System;
using NoZ;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
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
        [SerializeField] private GameObject _hud = null;
        [SerializeField] private GameObject _hudPlayerItem = null;
        [SerializeField] private RawImage _hudPlayerItemIcon = null;

        [Header("Screens")]
        [SerializeField] private UIScreen mainMenu = null;
        [SerializeField] private UIScreen worldsMenu = null;
        [SerializeField] private UIScreen ingameScreen = null;
        [SerializeField] private UIScreen puzzleComplete = null;

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
        }

        public static void Initialize ()
        {
            foreach (var screen in _instance.GetComponentsInChildren<UIScreen>(true))
                screen.gameObject.SetActive(false);

            _instance.startScreen.gameObject.SetActive(true);
            _instance.activeScreen = _instance.startScreen;

            _instance._cursor = CursorType.ArrowWithMinus;
            cursor = CursorType.Arrow;
        }

        public static void Shutdown ()
        {
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public static void ShowMainMenu()
        {
            if (_instance.activeScreen != null)
                _instance.activeScreen.gameObject.SetActive(false);

            _instance.activeScreen = _instance.mainMenu;
            _instance.activeScreen.gameObject.SetActive(true);
        }

        public static void ShowIngame ()
        {
            if (_instance.activeScreen != null)
                _instance.activeScreen.gameObject.SetActive(false);

            _instance.activeScreen = _instance.ingameScreen;
            _instance.activeScreen.gameObject.SetActive(true);
        }

        public static void ShowWorldsMenu()
        {
            _instance.activeScreen = _instance.worldsMenu;
            _instance.activeScreen.gameObject.SetActive(true);
        }

        public void ShowPuzzleComplete()
        {
            // Special case for finishing a puzzle when the editor is open
            if(UIPuzzleEditor.isOpen)
            {
                UIPuzzleEditor.Stop ();
                return;
            }

            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            activeScreen = puzzleComplete;
            activeScreen.gameObject.SetActive(true);            
        }
        
        public static void HideMenu ()
        {
            if (null == _instance.activeScreen)
                return;

            _instance.activeScreen.gameObject.SetActive(false);
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
    }
}
