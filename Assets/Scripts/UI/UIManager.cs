using System;
using UnityEngine;

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

        [Header("Screens")]
        [SerializeField] private UIScreen mainMenu = null;
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

        public void ShowMainMenu()
        {
            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            activeScreen = mainMenu;
            activeScreen.gameObject.SetActive(true);
        }

        public void ShowIngame ()
        {
            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            activeScreen = ingameScreen;
            activeScreen.gameObject.SetActive(true);
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

        public static UIPopup ShowPopup(UIPopup popupPrefab)
        {
            _instance.popups.gameObject.SetActive(true);
            return Instantiate(popupPrefab, _instance.popupCentered).GetComponent<UIPopup>();
        }

        public static void ClosePopup()
        {
            _instance.popups.gameObject.SetActive(false);
            _instance.popupCentered.DetachAndDestroyChildren();
        }
    }
}
