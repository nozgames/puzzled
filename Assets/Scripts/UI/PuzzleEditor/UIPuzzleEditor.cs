using System;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

using Puzzled.Editor;
using Puzzled.UI;

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
        [SerializeField] private SelectionGizmo selectionGizmo = null;
        [SerializeField] private SelectionGizmo _cursorGizmo = null;
        [SerializeField] private TMPro.TextMeshProUGUI puzzleName = null;
        [SerializeField] private Button playButton = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private GameObject dragWirePrefab = null;
        [SerializeField] private RectTransform _canvasCenter = null;

        [SerializeField] private Transform options = null;
        [SerializeField] private GameObject inspector = null;
        [SerializeField] private UIRadio[] layerToggles = null;
        [SerializeField] private UIRadio _gridToggle = null;

        [Header("Cameras")]
        [SerializeField] private Camera _cameraMain = null;
        [SerializeField] private Camera _cameraLogic = null;

        [Header("Toolbar")]
        [SerializeField] private GameObject tools = null;
        [SerializeField] private UIRadio moveTool = null;
        [SerializeField] private UIRadio drawTool = null;
        [SerializeField] private UIRadio eraseTool = null;
        [SerializeField] private UIRadio wireTool = null;
        [SerializeField] private UIRadio decalTool = null;
        [SerializeField] private GameObject moveToolOptions = null;
        [SerializeField] private GameObject eraseToolOptions = null;
        [SerializeField] private Toggle eraseToolAllLayers = null;        


        [Header("Popups")]
        [SerializeField] private GameObject popups = null;
        [SerializeField] private GameObject fileMenuPopup = null;
        [SerializeField] private UIChoosePuzzlePopup _chooseFilePopup = null;
        [SerializeField] private GameObject _chooseTilePopup = null;
        [SerializeField] private UITilePalette _chooseTilePalette = null;
        [SerializeField] private GameObject _chooseDecalPopup = null;
        [SerializeField] private UIDecalPalette _chooseDecalPalette = null;
        [SerializeField] private GameObject _chooseBackgroundPopup = null;
        [SerializeField] private UIBackgroundPalette _chooseBackgroundPalette = null;
        [SerializeField] private GameObject _chooseSoundPopup = null;
        [SerializeField] private UISoundPalette _chooseSoundPalette= null;
        [SerializeField] private UIPortSelector _choosePortPopup = null;
        [SerializeField] private UITileSelector _chooseTileConnectionPopup = null;

        private Mode _mode = Mode.Unknown;

        private Puzzle _puzzle = null;
        private bool playing;
        private Action<Tile> _chooseTileCallback;
        private Action<Decal> _chooseDecalCallback;
        private Action<Background> _chooseBackgroundCallback;
        private Action<Sound> _chooseSoundCallback;
        private Mode savedMode;
        private Cell _selectionMin;
        private Cell _selectionMax;
        private Cell _selectionSize;
        private Action<KeyCode> _onKey;

        public static UIPuzzleEditor instance { get; private set; }

        public Puzzle puzzle => _puzzle;

        public bool hasSelection => selectionGizmo.gameObject.activeSelf;

        public static bool isOpen => instance != null;

        public Mode mode {
            get => _mode;
            set {
                if (_mode == value)
                    return;

                selectionGizmo.gameObject.SetActive(false);

                _mode = value;

                switch (_mode)
                {
                    case Mode.Draw:
                        drawTool.isOn = true;
                        break;
                    case Mode.Move:
                        moveTool.isOn = true;
                        break;
                    case Mode.Erase:
                        eraseTool.isOn = true;
                        break;
                    case Mode.Logic:
                        wireTool.isOn = true;
                        break;
                    case Mode.Decal:
                        decalTool.isOn = true;
                        break;
                }

                UpdateMode();
            }
        }

        public static void Initialize()
        {
            UIManager.loading = true;
            UIManager.HideMenu();
            SceneManager.LoadSceneAsync("Editor", LoadSceneMode.Additive).completed += (handle) => {
                UIManager.loading = false;
            };
        }

        public static void Shutdown()
        {
            if (instance == null)
                return;

            instance.gameObject.SetActive(false);
            SceneManager.UnloadSceneAsync("Editor");
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
                case Mode.Move:
                    EnableMoveTool();
                    break;
                case Mode.Draw:
                    EnableDrawTool();
                    break;
                case Mode.Erase:
                    EnableEraseTool();
                    break;
                case Mode.Logic:
                    EnableLogicTool();
                    break;
                case Mode.Decal:
                    EnableDecalTool();
                    break;
            }

            UpdateCursor();
        }

        private void Awake()
        {
            instance = this;

            inspectorTileName.onEndEdit.AddListener(OnInspectorTileNameChanged);

            _gridToggle.onValueChanged.AddListener((v) => {
                _puzzle.showGrid = v;
            });

            _inspectorRotate.onValueChanged.AddListener((v) => {
                if (_selectedTile == null)
                    return;

                var rotated = _selectedTile.GetProperty("rotated");
                if (null == rotated)
                    return;

                ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(_selectedTile, "rotated", v));
            });

            _chooseFilePopup.onCancel += () => HidePopup();
            _chooseFilePopup.onOpenPuzzle += (filename) => {
                Load(filename);
                HidePopup();
            };
            _chooseFilePopup.onSaveFile += (filename) => {
                SaveAs(filename);
                HidePopup();
            };

            _chooseSoundPalette.onDoubleClickSound += (background) => {
                _chooseSoundCallback?.Invoke(background);
                HidePopup();
            };

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
            if(!TileDatabase.isLoaded || !DecalDatabase.isLoaded || !SFXDatabase.isLoaded || !BackgroundDatabase.isLoaded)
            {
                gameObject.SetActive(false);
                return;
            }

            popups.SetActive(false);

            KeyboardManager.Push(this);

            canvas.onScroll = OnScroll;
            canvas.onRButtonDrag = OnPan;

            GameManager.Stop();
            GameManager.busy++;

            inspector.SetActive(false);

            selectionGizmo.gameObject.SetActive(false);

            // Start off with an empty puzzle
            NewPuzzle();

            mode = Mode.Draw;
            drawTool.isOn = true;

            ClearUndo();

            InitializeCursor();

            // Uncomment to convert all files
            // UpgradeAllFiles();
        }

        private void OnScroll(Vector2 position, Vector2 delta)
        {
            if (playing || delta.y == 0)
                return;

            if(canvas.isMouseOver)
            CameraManager.AdjustZoom(delta.y > 0 ? -1 : 1);

            if (hasSelection)
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

            selectionGizmo.gameObject.SetActive(true);
            selectionGizmo.min = _puzzle.grid.CellToWorld(_selectionMin) - new Vector3(0.5f, 0, 0.5f);
            selectionGizmo.max = _puzzle.grid.CellToWorld(_selectionMax) + new Vector3(0.5f, 0, 0.5f);
        }

        private void OnPan(Vector2 position, Vector2 delta)
        {
            if (playing)
                return;

            CameraManager.Pan(-(canvas.CanvasToWorld(position + delta) - canvas.CanvasToWorld(position)));

            if (hasSelection)
                SetSelectionRect(_selectionMin, _selectionMax);

            UpdateCursor();
        }

        public void OnSaveButton()
        {
            if (!_puzzle.hasPath)
                OnSaveAsButton();
            else
            {
                Save();
                HidePopup();
            }
        }

        public void OnSaveAsButton()
        {
            ShowPopup(_chooseFilePopup.gameObject);
            _chooseFilePopup.SavePuzzle(_puzzle.path);
        }

        public void OnCancelPopup()
        {
            HidePopup();
        }

        public void SaveAs(string filename)
        {
            _puzzle.Save(filename);
            puzzleName.text = _puzzle.filename;
        }

        public void Save()
        {
            if (!_puzzle.hasPath)
                return;

            _puzzle.Save();
        }

        private void NewPuzzle()
        {
            if (_puzzle != null)
                _puzzle.Destroy();

            _puzzle = GameManager.InstantiatePuzzle();
            _puzzle.isEditing = true;
            _puzzle.showGrid = _gridToggle.isOn;
            Puzzle.current = _puzzle;

            // Default puzzle name to unnamed
            puzzleName.text = "Unnamed";

            selectionGizmo.gameObject.SetActive(false);

            // Reset the camera back to zero,zero
            CameraManager.camera = _cameraMain;
            CameraManager.Transition(CameraManager.defaultBackground, 0);
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
            ShowPopup(_chooseFilePopup.gameObject);
            _chooseFilePopup.OpenPuzzle();            
        }

        public static void Stop() => instance.OnStopButton();

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
                OnSaveAsButton();
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
            selectionGizmo.gameObject.SetActive(false);

            // Load the puzzle and play
            GameManager.LoadPuzzle(_puzzle.path);
            GameManager.Play();
        }

        private void Center(Cell cell, int zoomLevel = -1)
        {
            // Cente around the tile first
            CameraManager.Transition(puzzle.grid.CellToWorld(cell), zoomLevel, CameraManager.state.background, 0);

            // Now center around the actual center of the canvas
            var state = CameraManager.state;
            var center = canvas.CanvasToWorld(_canvasCenter.TransformPoint(Vector3.zero));
            var position = state.position - (center - state.position);
            CameraManager.Transition(position, zoomLevel, CameraManager.state.background, 0);
        }

        public void Load(string path)
        {
            try
            {
                if (_puzzle != null)
                    _puzzle.Destroy();

                _puzzle = GameManager.LoadPuzzle(path, true);
                _puzzle.showGrid = _gridToggle.isOn;

                var startingCamera = _puzzle.GetComponentsInChildren<StaticCamera>().Where(c => c.isStartingCamera).FirstOrDefault();
                var startingCell = Cell.zero;
                if (startingCamera != null)
                {
                    startingCell = startingCamera.tile.cell;
                    CameraManager.Transition(startingCamera.background, 0);                
                } else if (_puzzle.player != null)
                    startingCell = _puzzle.player.tile.cell;

                // Center the camera on the player
                Center(startingCell, CameraManager.DefaultZoomLevel);

                mode = Mode.Move;

                puzzleName.text = _puzzle.filename;
            } catch (Exception e)
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
            UpdateCursor();
        }

        private void HidePopup()
        {
            popups.SetActive(false);
            UpdateCursor();
        }

        [Flags]
        private enum GetTileFlag
        {
            None = 0,
            AllowInputs = 1,
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

        public void ChoosePort(Tile tileFrom, Tile tileTo, Action<Port, Port> callback)
        {
            ShowPopup(_choosePortPopup.gameObject);
            _choosePortPopup.GetComponent<RectTransform>().anchorMin =
            _choosePortPopup.GetComponent<RectTransform>().anchorMax = 
                CameraManager.camera.WorldToViewportPoint(tileTo.transform.position);
            _choosePortPopup.Open(tileFrom, tileTo, (from, to) => {
                HidePopup();
                callback?.Invoke(from, to);
            });
        }

        public void ChooseTileConnection (Tile[] tilesTo, Action<Tile> callback)
        {
            ShowPopup(_chooseTileConnectionPopup.gameObject);
            _chooseTileConnectionPopup.GetComponent<RectTransform>().anchorMin =
            _chooseTileConnectionPopup.GetComponent<RectTransform>().anchorMax =
                CameraManager.camera.WorldToViewportPoint(tilesTo[0].transform.position);
            _chooseTileConnectionPopup.Open(tilesTo, (target) => {
                HidePopup();
                callback?.Invoke(target);
            });
        }

        public void ChooseSound(Action<Sound> callback, Sound current)
        {
            _chooseSoundCallback = callback;
            _chooseSoundPalette.selected = current;
            ShowPopup(_chooseSoundPopup);
        }

        public void ChooseBackground(Action<Background> callback, Background current = null)
        {
            _chooseBackgroundCallback = callback;
            _chooseBackgroundPalette.selected = current;
            ShowPopup(_chooseBackgroundPopup);
        }

        public void ChooseDecal(Decal decal, Action<Decal> callback)
        {
            _chooseDecalCallback = callback;

            ShowPopup(_chooseDecalPopup);

            if (decal != Decal.none)
                _chooseDecalPalette.selected = decal;
        }

        public void ChooseTile(Type componentType, Tile selected, Action<Tile> callback)
        {
            _chooseTileCallback = callback;
            _chooseTilePalette.componentFilter = componentType;
            ShowPopup(_chooseTilePopup);

            if (selected != null)
                _chooseTilePalette.selected = selected;
        }

        public void CloseTileSelector(Tile tile)
        {
            if (tile != null)
                _chooseTileCallback?.Invoke(tile);

            HidePopup();
        }

        private void UpdateLayers()
        {
            for (int i = 0; i < layerToggles.Length; i++)
            {
                switch ((TileLayer)i)
                {
                    case TileLayer.Logic:
                        CameraManager.ShowLayer(_cameraLogic, (TileLayer)i, layerToggles[i].isOn);
                        break;

                    default:
                        CameraManager.ShowLayer(_cameraMain, (TileLayer)i, layerToggles[i].isOn);
                        break;
                }
            }
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

        /// <summary>
        /// Returns true if the given tile layer is visible in the editor
        /// </summary>
        /// <param name="layer">Given layer</param>
        /// <returns>True if the layer is visible</returns>
        public static bool IsLayerVisible(TileLayer layer) => instance.layerToggles[(int)layer].isOn;


        public void UpgradeAllFiles()
        {
            var files = Directory.GetFiles(Path.Combine(Application.dataPath, "Puzzles"), "*.puzzle", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var puzzle = Puzzle.Load(file);
                puzzle.Save();
                puzzle.Destroy();
            }
        }
    }
}
