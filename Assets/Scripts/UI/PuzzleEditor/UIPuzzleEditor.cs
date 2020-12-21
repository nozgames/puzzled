using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Puzzled.PuzzleEditor;

namespace Puzzled
{
    public partial class UIPuzzleEditor : UIScreen, KeyboardManager.IKeyboardHandler
    {
        public enum Mode
        {
            Unknown,
            Draw,
            Move,
            Erase,
            Logic,
            Decal
        }

        [Header("General")]
        [SerializeField] private UICanvas canvas = null;
        [SerializeField] private GameObject piecePrefab = null;
        [SerializeField] private RectTransform selectionRect = null;
        [SerializeField] private TMPro.TextMeshProUGUI puzzleName = null;
        [SerializeField] private Button playButton = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private GameObject dragWirePrefab = null;
        [SerializeField] private int minZoom = 1;
        [SerializeField] private int maxZoom = 10;
        [SerializeField] private GameObject fileButton = null;

        [SerializeField] private Transform options = null;
        [SerializeField] private GameObject inspector = null;

        [Header("Toolbar")]
        [SerializeField] private GameObject tools = null;
        [SerializeField] private Toggle moveTool = null;
        [SerializeField] private Toggle drawTool = null;
        [SerializeField] private Toggle eraseTool = null;
        [SerializeField] private Toggle wireTool = null;
        [SerializeField] private Toggle decalTool = null;
        [SerializeField] private GameObject moveToolOptions = null;
        [SerializeField] private GameObject eraseToolOptions = null;
        [SerializeField] private Toggle eraseToolAllLayers = null;
        [SerializeField] private Toggle[] layerToggles = null;

        [Header("Popups")]
        [SerializeField] private GameObject popups = null;
        [SerializeField] private GameObject fileMenuPopup = null;
        [SerializeField] private GameObject puzzleNamePopup = null;
        [SerializeField] private GameObject loadPopup = null;
        [SerializeField] private GameObject tileSelectorPopup = null;
        [SerializeField] private TMPro.TMP_InputField puzzleNameInput = null;
        [SerializeField] private Transform loadPopupFiles = null;
        [SerializeField] private GameObject loadPopupFilePrefab = null;
        [SerializeField] private Transform tileSelectorTiles = null;

        [Header("Inspector")]
        [SerializeField] private GameObject inspectorContent = null;
        [SerializeField] private TMPro.TMP_InputField inspectorTileName = null;
        [SerializeField] private RawImage inspectorTilePreview = null;
        [SerializeField] private UIOptionEditor optionPrefabInt = null;
        [SerializeField] private UIOptionEditor optionPrefabBool = null;
        [SerializeField] private UIOptionEditor optionPrefabString = null;
        [SerializeField] private UIOptionEditor optionPrefabTile = null;
        [SerializeField] private UIOptionEditor optionInputsPrefab = null;
        [SerializeField] private UIOptionEditor optionOutputsPrefab = null;
        [SerializeField] private GameObject optionPropertiesPrefab = null;

        [Header("Palette")]
        [SerializeField] private GameObject palette = null;
        [SerializeField] private UIList _paletteList = null;
        [SerializeField] private GameObject tileButtonPrefab = null;
        [SerializeField] private GameObject paletteDecalItemPrefab = null;

        private Mode _mode = Mode.Unknown;
        private string currentPuzzleName = null;
        private string puzzleToLoad = null;

        private Tile drawTile = null;
        private Cell dragStart;
        private Cell dragEnd;
        private bool dragging;
        private bool ignoreMouseUp;
        private bool playing;
        private bool panning;
        private Action<Tile> tileSelectorCallback;
        private Mode savedMode;

        private bool hasSelection => selectionRect.gameObject.activeSelf;
        private bool hasPuzzleName => !string.IsNullOrEmpty(currentPuzzleName);

        public static UIPuzzleEditor instance { get; private set; }


        public Mode mode {
            get => _mode;
            set {
                if (_mode == value)
                    return;

                selectionRect.gameObject.SetActive(false);
                
                _mode = value;

                switch (_mode)
                {
                    case Mode.Draw: 
                        drawTool.isOn = true; break;
                    case Mode.Move:
                        moveTool.isOn = true; break;
                    case Mode.Erase:
                        eraseTool.isOn = true; break;
                    case Mode.Logic:
                        wireTool.isOn = true; break;
                    case Mode.Decal:
                        decalTool.isOn = true; break;
                }

                UpdateMode();
            }
        }

        private void UpdateMode()
        {
            inspector.SetActive(false);
            palette.SetActive(false);

            _getCursor = null;

            canvas.UnregisterAll();

            DisableMoveTool();
            DisableDrawTool();
            DisableEraseTool();
            DisableLogicTool();
            DisableDecalTool();

            switch (_mode)
            {
                case Mode.Move: EnableMoveTool(); break;
                case Mode.Draw: EnableDrawTool(); break;
                case Mode.Erase: EnableEraseTool(); break;
                case Mode.Logic: EnableLogicTool(); break;
                case Mode.Decal: EnableDecalTool(); break;
            }

            UpdateCursor();
        }

        private void Awake()
        {
            instance = this;

            foreach (var toggle in layerToggles)
                toggle.onValueChanged.AddListener((value) => UpdateLayers());
        }

        private void OnEnable()
        {
            KeyboardManager.Push(this);

            canvas.onScroll = OnScroll;
            canvas.onRButtonDrag = OnPan;

            GameManager.Stop();
            GameManager.busy++;

            popups.SetActive(false);
            inspector.SetActive(false);            

            InitializePalette();

            selectionRect.gameObject.SetActive(false);

            mode = Mode.Draw;
            drawTool.isOn = true;

            ClearUndo();

            GameManager.UnloadPuzzle();
            currentPuzzleName = null;
            puzzleName.text = "Unnamed";

            InitializeCursor();
        }

        private void InitializePalette()
        {
            _paletteList.transform.DetachAndDestroyChildren();

            foreach (var tile in TileDatabase.GetTiles())
                GeneratePreview(tile);

            _decalNone = new Decal(Guid.Empty, null);
            var noneDecal = Instantiate(paletteDecalItemPrefab, _paletteList.transform).GetComponent<UIDecalItem>();
            noneDecal.decal = _decalNone;

            foreach (var decal in DecalDatabase.GetDecals())
            {
                var decalItem = Instantiate(paletteDecalItemPrefab, _paletteList.transform).GetComponent<UIDecalItem>();
                decalItem.decal = decal;
            }
        }

        private int FilterPalette(Type itemType)
        {
            var first = -1;
            for (int i = 0; i < _paletteList.transform.childCount; i++)
            {
                var child = _paletteList.transform.GetChild(i);
                child.gameObject.SetActive(child.GetComponent(itemType) != null);
                if (child.gameObject.activeSelf && first == -1)
                    first = i;
            }

            return first;
        }

        private void OnScroll(Vector2 position, Vector2 delta)
        {
            if (delta.y == 0)
                return;

            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + (delta.y > 0 ? -1 : 1), minZoom, maxZoom);

            if (selectedTile != null)
                SetSelectionRect(selectedTile.cell, selectedTile.cell);

            UpdateCursor(true);
        }

        private void OnDisable()
        {
            KeyboardManager.Pop();
            ClearUndo();

            if (!playing)
                Save();

            GameManager.busy--;
        }

        private void GeneratePreview(Tile prefab)
        {
            if (null == prefab)
                return;

            var t = TileDatabase.GetPreview(prefab.guid);

            var tileObject = Instantiate(piecePrefab, _paletteList.transform);
            var toggle = tileObject.GetComponent<Toggle>();
            toggle.group = _paletteList.transform.GetComponent<ToggleGroup>();
            toggle.onValueChanged.AddListener(v => {
                if (v)
                    drawTile = prefab;
            });
            tileObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = prefab.name;
            tileObject.GetComponentInChildren<RawImage>().texture = t;
        }

        public void OnToolChanged()
        {
            if (!GameManager.isValid)
                return;

            if (drawTool.isOn)
                mode = Mode.Draw;
            else if (moveTool.isOn)
                mode = Mode.Move;
            else if (eraseTool.isOn)
                mode = Mode.Erase;
            else if (wireTool.isOn)
                mode = Mode.Logic;
            else if (decalTool.isOn)
                mode = Mode.Decal;
        }

        private void SetSelectionRect(Cell min, Cell max)
        {
            var anchorCell = Cell.Min(min, max);
            var size = Cell.Max(min, max) - anchorCell;

            selectionRect.anchorMin = Camera.main.WorldToViewportPoint(TileGrid.CellToWorld(anchorCell) - new Vector3(0.5f, 0.5f, 0));
            selectionRect.anchorMax = Camera.main.WorldToViewportPoint(TileGrid.CellToWorld(anchorCell + size) + new Vector3(0.5f, 0.5f, 0));

            selectionRect.gameObject.SetActive(true);
        }

        private void OnPan (Vector2 position, Vector2 delta)
        {
            GameManager.Pan(canvas.CanvasToWorld(position + delta) - canvas.CanvasToWorld(position));

            if (selectedTile != null)
                SetSelectionRect(selectedTile.cell, selectedTile.cell);

            UpdateCursor();
        }

        public void OnSaveButton()
        {
            if (puzzleName.text == null || String.Compare(puzzleName.text, "unnamed", true) == 0)
                ShowPopup(puzzleNamePopup);
            else
            {
                Save();
                HidePopup();
            }
        }

        public void OnSaveAsButton()
        {
            ShowPopup(puzzleNamePopup);
        }

        public void OnSavePuzzleName()
        {
            if (string.IsNullOrEmpty(puzzleNameInput.text) || String.Compare(puzzleNameInput.text, "unnamed", true) == 0)
                return;

            puzzleName.text = puzzleNameInput.text;
            currentPuzzleName = puzzleName.text;

            HidePopup();
            Save();
        }

        public void OnCancelPopup()
        {
            HidePopup();
        }

        private string currentPuzzleFilename => 
            (string.IsNullOrEmpty(puzzleName.text) || string.Compare(puzzleName.text, "unnamed", true) == 0) ? 
                null : 
                Path.Combine(Application.dataPath, $"Puzzles/{puzzleName.text}.puzzle");

        public void Save()
        {
            if (null == currentPuzzleFilename)
                return;

            Puzzle.Save(TileGrid.GetLinkedTiles(), currentPuzzleFilename);
        }

        private void NewPuzzle()
        {
            selectionRect.gameObject.SetActive(false);
            currentPuzzleName = null;
            puzzleName.text = "Unnamed";
            GameManager.PanCenter();
            GameManager.UnloadPuzzle();
            Camera.main.orthographicSize = 6;
            ClearUndo();
            HidePopup();
        }

        public void OnNewButton()
        {
            NewPuzzle();
        }

        public void OnLoadButton()
        {
            loadPopupFiles.DetachAndDestroyChildren();

            var files = Directory.GetFiles(Path.Combine(Application.dataPath, "Puzzles"), "*.puzzle");
            foreach (var file in files)
            {
                var fileGameObject = Instantiate(loadPopupFilePrefab, loadPopupFiles);
                fileGameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(file);
                var item = fileGameObject.GetComponent<UIListItem>();
                item.onSelectionChanged.AddListener((selected) => {
                    if (!selected)
                        return;
                    puzzleToLoad = Path.GetFileNameWithoutExtension(file);                    
                });
                item.onDoubleClick.AddListener(() => {
                    OnLoadButtonConfirm();
                });
            }

            ShowPopup(loadPopup);
        }

        public void OnLoadButtonConfirm()
        {
            if (null != puzzleToLoad)
                Load(puzzleToLoad);
        }

        public void OnStopButton()
        {
            if (!playing)
                return;

            GameManager.Stop();
            GameManager.busy = 1;

            tools.SetActive(true);
            inspector.SetActive(mode == Mode.Logic);
            palette.SetActive(mode == Mode.Draw);
            moveTool.gameObject.SetActive(true);
            drawTool.gameObject.SetActive(true);
            eraseTool.gameObject.SetActive(true);
            wireTool.gameObject.SetActive(true);
            fileButton.gameObject.SetActive(true);

            playing = false;
            playButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            Load(currentPuzzleName);

            mode = savedMode;
            UpdateLayers();
        }

        public void OnPlayButton()
        {
            if (playing)
                return;

            if (!hasPuzzleName)
            {
                ShowPopup(puzzleNamePopup);
                return;
            }

            ClearUndo();
            tools.SetActive(false);
            savedMode = mode;
            mode = Mode.Unknown;
            inspector.SetActive(false);
            palette.SetActive(false);
            moveTool.gameObject.SetActive(false);
            drawTool.gameObject.SetActive(false);
            eraseTool.gameObject.SetActive(false);
            wireTool.gameObject.SetActive(false);
            fileButton.gameObject.SetActive(false);

            GameManager.busy = 0;

            playButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

            playing = true;
            Save();

            // Clear selection
            selectionRect.gameObject.SetActive(false);

            GameManager.Play();
        }

        public void Load(string file)
        {
            NewPuzzle();

            currentPuzzleName = file;
            puzzleName.text = currentPuzzleName;
            Puzzle.Load(currentPuzzleFilename);
            HidePopup();
            UpdateMode();
        }

        public void OnFileMenuButton()
        {
            ShowPopup(fileMenuPopup);
        }

        private void ShowPopup(GameObject popup)
        {
            if (popup.transform.parent != popups.transform)
                return;

            popups.transform.DisableChildren();
            popups.SetActive(true);
            popup.SetActive(true);
        }

        private void HidePopup()
        {
            popups.SetActive(false);
        }

        private Tile GetTile(Cell cell, TileLayer topLayer = TileLayer.Logic)
        {
            for (int i = (int)topLayer; i >= 0; i--)
            {
                var tile = TileGrid.CellToTile(cell, (TileLayer)i);
                if (null == tile)
                    continue;

                // Do not return tiles on hidden layers
                if (!layerToggles[i].isOn)
                    continue;

                return tile;
            }

            return null;
        }

        private void RemoveWire(Wire wire)
        {
            Destroy(wire.gameObject);
        }

        private UIOptionEditor InstantiateOptionEditor(Type type, Transform parent)
        {
            if (type == typeof(int))
                return Instantiate(optionPrefabInt, parent).GetComponent<UIOptionEditor>();
            else if (type == typeof(bool))
                return Instantiate(optionPrefabBool, parent).GetComponent<UIOptionEditor>();
            else if (type == typeof(string))
                return Instantiate(optionPrefabString, parent).GetComponent<UIOptionEditor>();
            else if (type == typeof(Guid))
                return Instantiate(optionPrefabTile, parent).GetComponent<UIOptionEditor>();

            return null;
        }

        public void OpenTileSelector(Type componentType, Action<Tile> callback)
        {
            tileSelectorCallback = callback;

            ShowPopup(tileSelectorPopup);

            tileSelectorTiles.DetachAndDestroyChildren();

            foreach(var tile in TileDatabase.GetTiles().Where(t => t.GetComponent(componentType) != null))
            {
                var tileButton = Instantiate(tileButtonPrefab, tileSelectorTiles).GetComponent<UITileItem>();
                tileButton.tile = tile;
                tileButton.GetComponent<Toggle>().onValueChanged.AddListener((value) => {
                    CloseTileSelector(tile);                    
                });
            }
        }

        public void CloseTileSelector (Tile tile)
        {
            if (tile != null)
                tileSelectorCallback?.Invoke(tile);

            HidePopup();
        }        

        private void UpdateLayers()
        {
            for(int i=0;i<layerToggles.Length; i++)
                GameManager.ShowLayer((TileLayer)i, layerToggles[i].isOn);
        }

        void KeyboardManager.IKeyboardHandler.OnKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Escape:
                    ShowPopup(fileMenuPopup);
                    break;

                case KeyCode.Z:
                    if (KeyboardManager.isCtrlPressed && !KeyboardManager.isShiftPressed)
                        Undo();
                    else if (KeyboardManager.isCtrlPressed && KeyboardManager.isShiftPressed)
                        Redo();
                    break;

                case KeyCode.Y:
                    if (KeyboardManager.isCtrlPressed)
                        Redo();
                    break;
            }
        }

        void KeyboardManager.IKeyboardHandler.OnModifiersChanged(bool shift, bool ctrl, bool alt)
        {
            switch (mode)
            {
                case Mode.Erase:
                    OnModifiersChangedErase(shift, ctrl, alt);
                    break;
            }

            UpdateCursor();
        }
    }
}
