using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Puzzled.Editor;

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
        [SerializeField] private RectTransform _canvasCenter = null;
        [SerializeField] private RectTransform selectionRect = null;
        [SerializeField] private TMPro.TextMeshProUGUI puzzleName = null;
        [SerializeField] private Button playButton = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private GameObject dragWirePrefab = null;
        [SerializeField] private int minZoom = 1;
        [SerializeField] private int maxZoom = 10;

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
        [SerializeField] private TMPro.TMP_InputField puzzleNameInput = null;
        [SerializeField] private Transform loadPopupFiles = null;
        [SerializeField] private GameObject loadPopupFilePrefab = null;
        [SerializeField] private GameObject _chooseTilePopup = null;
        [SerializeField] private UITilePalette _chooseTilePalette = null;
        [SerializeField] private GameObject _chooseDecalPopup= null;
        [SerializeField] private UIDecalPalette _chooseDecalPalette = null;
        [SerializeField] private GameObject _chooseBackgroundPopup = null;
        [SerializeField] private UIBackgroundPalette _chooseBackgroundPalette = null;

        private Mode _mode = Mode.Unknown;

        private Puzzle _puzzle = null;
        private bool playing;
        private Action<Tile> _chooseTileCallback;
        private Action<Decal> _chooseDecalCallback;
        private Action<Background> _chooseBackgroundCallback;
        private Mode savedMode;
        private Cell _selectionMin;
        private Cell _selectionMax;
        private Cell _selectionSize;
        private Action<KeyCode> _onKey;

        public static UIPuzzleEditor instance { get; private set; }

        public Puzzle puzzle => _puzzle;

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

            _getCursor = null;
            _onKey = null;

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

            _chooseBackgroundPalette.onDoubleClickBackground += (background) => {
                _chooseBackgroundCallback?.Invoke(background);
                HidePopup();
            };

            _chooseDecalPalette.onDoubleClickDecal += (decal) => {
                _chooseDecalCallback?.Invoke(decal);
                HidePopup();
            };

            _chooseTilePalette.onDoubleClickTile += (tile) => {
                _chooseTileCallback?.Invoke(tile);
                HidePopup();
            };

            foreach (var toggle in layerToggles)
                toggle.onValueChanged.AddListener((value) => {
                    UpdateLayers();
                    FitSelectionRect();
                });
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

            selectionRect.gameObject.SetActive(false);

            // Start off with an empty puzzle
            NewPuzzle();

            mode = Mode.Draw;
            drawTool.isOn = true;

            ClearUndo();

            InitializeCursor();
        }

        private void OnScroll(Vector2 position, Vector2 delta)
        {
            if (playing || delta.y == 0)
                return;

            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + (delta.y > 0 ? -1 : 1), minZoom, maxZoom);

            if (selectionRect.gameObject.activeSelf)
                SetSelectionRect(_selectionMin, _selectionMax);

            UpdateCursor(true);
        }

        private void OnDisable()
        {
            KeyboardManager.Pop();
            ClearUndo();

            if (!playing)
                Save();

            if (_puzzle != null)
            {
                _puzzle.Destroy();
                _puzzle = null;
            }

            GameManager.busy--;
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

        public void SetSelectionRect(Cell min, Cell max)
        {
            _selectionMin = Cell.Min(min, max);
            _selectionMax = Cell.Max(min, max);
            _selectionSize = _selectionMax - _selectionMin; 

            selectionRect.anchorMin = Camera.main.WorldToViewportPoint(_puzzle.grid.CellToWorld(_selectionMin) - new Vector3(0.5f, 0.5f, 0));
            selectionRect.anchorMax = Camera.main.WorldToViewportPoint(_puzzle.grid.CellToWorld(_selectionMax) + new Vector3(0.5f, 0.5f, 0));

            selectionRect.gameObject.SetActive(true);
        }

        private void OnPan (Vector2 position, Vector2 delta)
        {
            if (playing)
                return;

            CameraManager.Pan(-(canvas.CanvasToWorld(position + delta) - canvas.CanvasToWorld(position)));

            if(selectionRect.gameObject.activeSelf)
                SetSelectionRect(_selectionMin, _selectionMax);

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
            // Dont allow an empty puzzle name or one named "unnamed"
            if (string.IsNullOrEmpty(puzzleNameInput.text) || string.Compare(puzzleNameInput.text, "unnamed", true) == 0)
                return;

            // Save the puzzle with the new name
            _puzzle.Save(Path.Combine(Application.dataPath, $"Puzzles/{puzzleNameInput.text}.puzzle"));

            puzzleName.text = _puzzle.filename;

            HidePopup();
            Save();
        }

        public void OnCancelPopup()
        {
            HidePopup();
        }

        public void Save()
        {
            if (!_puzzle.hasPath)
                return;

            _puzzle.Save();
        }

        private void NewPuzzle()
        {
            if(_puzzle != null)
                _puzzle.Destroy();

            _puzzle = GameManager.InstantiatePuzzle();
            _puzzle.isEditing = true;
            _puzzle.showGrid = true;
            Puzzle.current = _puzzle;

            // Default puzzle name to unnamed
            puzzleName.text = "Unnamed";

            selectionRect.gameObject.SetActive(false);

            // Reset the camera back to zero,zero
            CameraManager.TransitionToBackground(null, 0);
            Center(new Cell(0, 0), CameraManager.DefaultZoomLevel);
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
                });
                item.onDoubleClick.AddListener(() => {
                    Load(file);
                });
            }

            ShowPopup(loadPopup);
        }

        public void OnStopButton()
        {
            if (!playing)
                return;

            // Stop playing and unload the puzzle
            GameManager.Stop();
            GameManager.UnloadPuzzle();
            GameManager.busy = 1;

            tools.SetActive(true);
            inspector.SetActive(mode == Mode.Logic);

            playing = false;
            playButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);

            // Set our editing puzzle as active
            Puzzle.current = _puzzle;

            // Return to the saved mode
            mode = savedMode;
            UpdateLayers();
        }

        public void OnPlayButton()
        {
            // Do not allow playing if already playing
            if (playing)
                return;

            // Prompt for saving if it has not yet been saved
            if (!_puzzle.hasPath)
            {
                ShowPopup(puzzleNamePopup);
                return;
            }

            // We have to save the puzzle before we can play because
            // play will load the puzzle
            Save();

            tools.SetActive(false);
            savedMode = mode;
            mode = Mode.Unknown;
            inspector.SetActive(false);

            GameManager.busy = 0;

            playButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

            playing = true;

            // Clear selection
            selectionRect.gameObject.SetActive(false);

            // Load the puzzle and play
            GameManager.LoadPuzzle(_puzzle.path);
            GameManager.Play();
        }

        private void Center(Cell cell, int zoomLevel=-1)
        {
            // Cente around the tile first
            CameraManager.JumpToCell(cell, zoomLevel);

            // Offset the camera by the center of the canvas
            var state = CameraManager.state;
            state.position -= (CameraManager.ScreenToWorld(_canvasCenter.TransformPoint(Vector3.zero)) - state.position);
            CameraManager.state = state;
        }

        public void Load(string path)
        {
            try
            {
                if (_puzzle != null)
                    _puzzle.Destroy();

                _puzzle = GameManager.LoadPuzzle(path, true);
                _puzzle.showGrid = true;

                // Center the camera on the player
                Center(CameraManager.cell, CameraManager.DefaultZoomLevel);

                puzzleName.text = _puzzle.filename;
            } 
            catch (Exception e)
            {
                Debug.LogException(e);
                NewPuzzle();
                return;
            }

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

        [Flags]
        private enum GetTileFlag
        {
            None = 0,
            AllowInputs  = 1,
            AllowOutputs = 2
        }

        private Tile GetTile(Cell cell, GetTileFlag flags) => GetTile(cell, TileLayer.Logic, flags);

        private Tile GetTile(Cell cell, TileLayer topLayer = TileLayer.Logic, GetTileFlag flags = GetTileFlag.None)
        {
            for (int i = (int)topLayer; i >= 0; i--)
            {
                var tile = _puzzle.grid.CellToTile(cell, (TileLayer)i);
                if (null == tile)
                    continue;

#if false
                if (!tile.info.allowWireInputs && (flags & GetTileFlag.AllowInputs) == GetTileFlag.AllowInputs)
                    continue;

                if (!tile.info.allowWireOutputs && (flags & GetTileFlag.AllowOutputs) == GetTileFlag.AllowOutputs)
                    continue;
#endif

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

        public void ChooseBackground(Action<Background> callback, Background current = null)
        {
            _chooseBackgroundCallback = callback;
            _chooseBackgroundPalette.selected = current;
            ShowPopup(_chooseBackgroundPopup);
        }

        public void ChooseDecal (Action<Decal> callback)
        {
            _chooseDecalCallback = callback;
            ShowPopup(_chooseDecalPopup);
        }

        public void ChooseTile (Type componentType, Action<Tile> callback)
        {
            _chooseTileCallback = callback;
            _chooseTilePalette.componentFilter = componentType;
            ShowPopup(_chooseTilePopup);
        }

        public void CloseTileSelector (Tile tile)
        {
            if (tile != null)
                _chooseTileCallback?.Invoke(tile);

            HidePopup();
        }        

        private void UpdateLayers()
        {
            for(int i=0;i<layerToggles.Length; i++)
                CameraManager.ShowLayer((TileLayer)i, layerToggles[i].isOn);
        }

        void KeyboardManager.IKeyboardHandler.OnKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Escape:
                    if (playing)
                        OnStopButton();
                    else
                        ShowPopup(fileMenuPopup);
                    break;
            }

            if (playing)
                return;

            switch (keyCode)
            {
                case KeyCode.W:
                    mode = Mode.Logic;
                    break;

                case KeyCode.B:
                    mode = Mode.Draw;
                    break;

                case KeyCode.E:
                    mode = Mode.Erase;
                    break;

                case KeyCode.V:
                    mode = Mode.Move;
                    break;

                case KeyCode.D:
                    mode = Mode.Decal;
                    break;

                case KeyCode.Alpha1:
                    layerToggles[0].isOn = !layerToggles[0].isOn;
                    break;

                case KeyCode.Alpha2:
                    layerToggles[1].isOn = !layerToggles[1].isOn;
                    break;

                case KeyCode.Alpha3:
                    layerToggles[2].isOn = !layerToggles[2].isOn;
                    break;

                case KeyCode.Alpha4:
                    layerToggles[3].isOn = !layerToggles[3].isOn;
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

            _onKey?.Invoke(keyCode);
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
