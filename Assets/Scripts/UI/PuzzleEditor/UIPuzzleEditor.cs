using System;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

using Puzzled.Editor;
using Puzzled.UI;

namespace Puzzled
{
    public partial class UIPuzzleEditor : UIScreen, KeyboardManager.IKeyboardHandler
    {
        public const int DefaultZoom = 12;
        public const int MaxZoom = 30;

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
        [SerializeField] private TMPro.TextMeshProUGUI puzzleName = null;
        [SerializeField] private Button playButton = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private GameObject dragWirePrefab = null;
        [SerializeField] private RectTransform _canvasCenter = null;
        [SerializeField] private Slider _zoomSlider = null;
        [SerializeField] private Slider _pitchSlider = null;
        [SerializeField] private Slider _yawSlider = null;
        [SerializeField] private UICameraEditor _cameraEditor = null;

        [SerializeField] private GameObject inspector = null;
        [SerializeField] private UIRadio _layerToggleLogic = null;
        [SerializeField] private UIRadio _layerToggleFloor = null;
        [SerializeField] private UIRadio _layerToggleWall = null;
        [SerializeField] private UIRadio _layerToggleDynamic = null;
        [SerializeField] private UIRadio _layerToggleStatic = null;
        [SerializeField] private UIRadio _gridToggle = null;
        [SerializeField] private UIRadio _wireToggle = null;
        [SerializeField] private UIRadio _debugToggle = null;
        [SerializeField] private GameObject _canvasControls = null;

        [SerializeField] private UITooltipPopup _tooltip = null;
        [SerializeField] private GameObject _playControls = null;

        [Header("Gizmos")]
        [SerializeField] private SelectionGizmo _selectionGizmo = null;
        [SerializeField] private SelectionGizmo _cursorGizmo = null;

        [Header("Toolbar")]
        [SerializeField] private GameObject _toolbar = null;
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
        [SerializeField] private UIImportPopup _importPopup = null;
        [SerializeField] private UIColorPicker _colorPicker = null;

        private Mode _mode = Mode.Unknown;

        private Puzzle _puzzle = null;
        private bool playing;
        private Action<Tile> _chooseTileCallback;
        private Action<Decal> _chooseDecalCallback;
        private Action<Background> _chooseBackgroundCallback;
        private Action<Sound> _chooseSoundCallback;
        private Mode savedMode;
        private Action<KeyCode> _onKey;

        private Vector3 _cameraTarget;
        private int _cameraZoom = DefaultZoom;

        public static UIPuzzleEditor instance { get; private set; }

        public Puzzle puzzle => _puzzle;

        public static bool isOpen => instance != null;

        public static bool isDebugging => instance.playing && instance._debugToggle.isOn;

        public Mode mode {
            get => _mode;
            set {
                if (_mode == value)
                    return;

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

            instance._pointerAction.action.performed -= instance.OnPointerMoved;

            instance.gameObject.SetActive(false);
            SceneManager.UnloadSceneAsync("Editor");

            UIManager.ShowMainMenu();
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

            // Clear selection if the inspector is not enabled
            if (!inspector.gameObject.activeSelf)
            {
                puzzle.HideWires();
                SelectTile(null);
            }

            UpdateCursor();
        }

        private void Awake()
        {
            instance = this;

            inspectorTileName.onEndEdit.AddListener(OnInspectorTileNameChanged);

            _zoomSlider.minValue = CameraManager.MinZoom;
            _zoomSlider.maxValue = MaxZoom;
            _zoomSlider.value = _cameraZoom;
            _zoomSlider.onValueChanged.AddListener((v) => {
                UpdateZoom((int)v);
;            });

            _pitchSlider.value = CameraManager.DefaultPitch;
            _pitchSlider.onValueChanged.AddListener((v) => {
                UpdateCamera();
            });

            _yawSlider.value = CameraManager.DefaultYaw;
            _yawSlider.onValueChanged.AddListener((v) => {
                UpdateCamera();
            });

            _gridToggle.onValueChanged.AddListener((v) => {
                _puzzle.showGrid = v;
            });

            _wireToggle.onValueChanged.AddListener((v) => CameraManager.ShowWires(v));

            _inspectorRotation.onClick.AddListener(() => {
                var rotation = selectedTile.GetProperty("rotation");
                if (null != rotation)
                    ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(selectedTile, "rotation", rotation.GetValue<int>(selectedTile) + 1));
            });

            _inspectorFlip.onValueChanged.AddListener((v) => {
                if (selectedTile == null)
                    return;

                var flip = selectedTile.GetProperty("flipped");
                if (null == flip)
                    return;

                ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(selectedTile, "flipped", v));
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

            _layerToggleWall.onValueChanged.AddListener((v) => UpdateCameraFlags());
            _layerToggleDynamic.onValueChanged.AddListener((v) => UpdateCameraFlags());
            _layerToggleLogic.onValueChanged.AddListener((v) => UpdateCameraFlags());
            _layerToggleStatic.onValueChanged.AddListener((v) => UpdateCameraFlags());
            _layerToggleFloor.onValueChanged.AddListener((v) => UpdateCameraFlags());
        }

        private void OnEnable()
        {
            popups.SetActive(false);

            KeyboardManager.Push(this);

            canvas.onScroll = OnScroll;
            canvas.onRButtonDrag = OnPan;

            GameManager.Stop();
            GameManager.busy++;

            inspector.SetActive(false);

            _selectionGizmo.gameObject.SetActive(false);

            // Start off with an empty puzzle
            NewPuzzle();

            mode = Mode.Draw;
            drawTool.isOn = true;

            ClearUndo();

            InitializeCursor();

            UpdateCameraFlags();

            // Uncomment to convert all files
            // UpgradeAllFiles();
        }

        private void OnScroll(Vector2 position, Vector2 delta)
        {
            if (playing || delta.y == 0 || !canvas.isMouseOver)
                return;

            UpdateZoom(_cameraZoom + (delta.y > 0 ? -1 : 1));
        }

        private void UpdateZoom(int zoom)
        {
            _cameraZoom = Mathf.Clamp(zoom, CameraManager.MinZoom, MaxZoom);

            UpdateCamera();
            UpdateSelectionGizmo();

            _zoomSlider.SetValueWithoutNotify(_cameraZoom);

            UpdateCursor(true);
        }

        private void OnDisable()
        {
            KeyboardManager.Pop();
            ClearUndo();

            if (!playing && puzzle.isModified)
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

        private void OnPan(Vector2 position, Vector2 delta)
        {
            // Do not allow panning in play mode
            if (playing)
                return;

            _cameraTarget += -(canvas.CanvasToWorld(position + delta) - canvas.CanvasToWorld(position));

            UpdateCamera();
            UpdateSelectionGizmo();
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

            // Add a star to the end of the puzzle name
            if (instance.puzzleName.text.EndsWith("*"))
                instance.puzzleName.text = instance.puzzleName.text.Substring(0, instance.puzzleName.text.Length - 1);
        }

        private void NewPuzzle()
        {
            if (_puzzle != null && _puzzle.isModified)
                Save();

            if (_puzzle != null)
                _puzzle.Destroy();

            _puzzle = GameManager.InstantiatePuzzle();
            _puzzle.isEditing = true;
            _puzzle.showGrid = _gridToggle.isOn;
            Puzzle.current = _puzzle;

            // Default puzzle name to unnamed
            puzzleName.text = "Unnamed";

            ClearSelection();

            // Reset the camera back to zero,zero
            Center(new Cell(0, 0), DefaultZoom);
            ClearUndo();
            HidePopup();

            mode = Mode.Draw;
        }

        public void OnExitButton()
        {
            if (_puzzle != null && _puzzle.isModified)
                Save();

            Shutdown();
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


        public void OnPlayButton() => BeginPlay();
        public void OnStopButton() => EndPlay();
        public void OnDebugButton()
        {
            if (!playing)
                return;

            GameManager.puzzle.ShowWires(isDebugging);
            CameraManager.ShowWires(isDebugging);
            CameraManager.ShowLayer(TileLayer.Logic, isDebugging);
        }

        private void BeginPlay()
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
            if(puzzle.isModified)
                Save();

            _toolbar.SetActive(false);
            _playControls.SetActive(true);
            savedMode = mode;
            mode = Mode.Unknown;
            inspector.SetActive(false);
            _canvasControls.SetActive(false);

            GameManager.busy = 0;

            playButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

            playing = true;

            ClearSelection();

            // Load the puzzle and play
            GameManager.LoadPuzzle(_puzzle.path);
            GameManager.Play();

            if (isDebugging)
                OnDebugButton();
        }

        private void EndPlay ()
        {
            if (!playing)
                return;

            // Stop playing and unload the puzzle
            GameManager.Stop();
            GameManager.UnloadPuzzle();
            GameManager.busy = 1;

            _toolbar.SetActive(true);
            _playControls.SetActive(false);
            inspector.SetActive(mode == Mode.Logic);

            playing = false;
            playButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            _canvasControls.SetActive(true);

            // Set our editing puzzle as active
            Puzzle.current = _puzzle;

            // Return to the saved mode
            mode = savedMode;

            UpdateCameraFlags();
        }

        private void UpdateCamera()
        {
            CameraManager.editorCameraState = new GameCamera.State
            {
                targetPosition = _cameraTarget,
                pitch = _pitchSlider.value, 
                yaw = _yawSlider.value * 90,
                yawIndex = 0,
                zoomLevel = _cameraZoom,
                bgColor = Color.black
            };
        }

        private void Center(Cell cell, int zoom = -1)
        {
            _cameraTarget = _puzzle.grid.CellToWorld(cell);
            _cameraZoom = zoom == -1 ? _cameraZoom : zoom;

            UpdateCamera();
#if false
            // Center around the tile first
            CameraManager.Transition(
                puzzle.grid.CellToWorld(cell), 
                CameraManager.DefaultPitch,
                zoom == -1 ? state.zoom : zoom,
                null,
                0);

            // Now center around the actual center of the canvas
            state = CameraManager.state;
            CameraManager.Transition(
                state.target - (canvas.CanvasToWorld(_canvasCenter.TransformPoint(Vector3.zero)) - state.target),
                CameraManager.DefaultPitch,
                state.zoom,
                null,
                0);
#endif //false

        }

        public void Load(string path)
        {
            try
            {
                if (_puzzle != null)
                    _puzzle.Destroy();

                _puzzle = GameManager.LoadPuzzle(path, true);
                _puzzle.showGrid = _gridToggle.isOn;

                mode = Mode.Logic;

                ClearSelection();

                puzzleName.text = _puzzle.filename;

                Vector3 targetPosition;

                // Center on starting camera if there is one
                if (_puzzle.properties.startingCamera != null)
                    targetPosition = _puzzle.properties.startingCamera.target;
                else if (_puzzle.player != null) // If we have a player center on the player
                    targetPosition = _puzzle.grid.CellToWorld(_puzzle.player.tile.cell);
                else // Otherwise center on the middle of all tiles
                {
                    var min = _puzzle.grid.maxCell;
                    var max = _puzzle.grid.minCell;
                    foreach (var tile in puzzle.grid.GetTiles())
                    {
                        // Special case to skip the puzzle properties
                        if (tile.cell == _puzzle.grid.minCell)
                            continue;

                        min = Cell.Min(min, tile.cell);
                        max = Cell.Max(max, tile.cell);
                    }

                    targetPosition = _puzzle.grid.CellToWorld(new Cell((max.x + min.x) / 2, (max.y + min.y) / 2));
                }

                _cameraTarget = targetPosition;
                UpdateCamera();

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


        /// <summary>
        /// Returns the top most tile in the given cell that has a property with the given name
        /// </summary>
        /// <param name="cell">Cell to search</param>
        /// <param name="propertyName">Name of property to search for</param>
        /// <param name="topLayer">Layer to start searching from</param>
        /// <returns>Topmost tile in the cell with the given property or null</returns>
        public Tile GetTopMostTileWithProperty(Cell cell, string propertyName, TileLayer topLayer = TileLayer.Logic)
        {
            for (int i = (int)topLayer; i >= 0; i--)
            {
                var tile = _puzzle.grid.CellToTile(cell, (TileLayer)i);
                if (null == tile)
                    continue;

                // Do not return tiles on hidden layers
                if (!IsLayerVisible((TileLayer)i))
                    continue;

                if (tile.GetProperty(propertyName) == null)
                    continue;

                return tile;
            }

            return null;
        }
        
        private Tile GetTopMostTile(Cell cell, TileLayer topLayer = TileLayer.Logic)
        {
            for (int i = (int)topLayer; i >= 0; i--)
            {
                var tile = _puzzle.grid.CellToTile(cell, (TileLayer)i);
                if (null == tile)
                    continue;

                // Do not return tiles on hidden layers
                if (!IsLayerVisible((TileLayer)i))
                    continue;

                return tile;
            }

            return null;
        }

        /// <summary>
        /// Returns the next tile in the same cell in reverse layer order.
        /// </summary>
        /// <param name="tile">Current tile</param>
        /// <returns>Next tile in reverse layer order</returns>
        private Tile GetNextTile (Cell cell, TileLayer layer, CellCoordinateSystem system)
        {
            if (cell == Cell.invalid)
                return null;
            
            cell = cell.ConvertTo(system);

            var nextTile = GetTopMostTile(cell, layer != TileLayer.Floor ? (layer - 1) : TileLayer.Logic);
            if (null == nextTile)
            {
                switch (cell.system)
                {
                    case CellCoordinateSystem.Edge:
                        return GetNextTile(cell, TileLayer.Logic, CellCoordinateSystem.SharedEdge);

                    case CellCoordinateSystem.SharedEdge:
                        return GetNextTile(cell, TileLayer.Logic, CellCoordinateSystem.Grid);

                    default:
                        nextTile = GetTopMostTile(cell, TileLayer.Logic);
                        break;
                }
            }                    

            return nextTile;
        }

        public void ChoosePort(Tile tileFrom, Tile tileTo, Action<Port, Port> callback)
        {
            ShowPopup(_choosePortPopup.gameObject);
            _choosePortPopup.GetComponent<RectTransform>().anchorMin =
            _choosePortPopup.GetComponent<RectTransform>().anchorMax = 
                CameraManager.camera.WorldToViewportPoint(tileTo.transform.position);
            _choosePortPopup.Open(tileFrom, tileTo, (from, to) => {
                HidePopup();
                if(from != null && to != null)
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

        private void UpdateCameraFlags()
        {
            CameraManager.ShowLetterbox(false);
            CameraManager.ShowGizmos();
            CameraManager.ShowWires(_wireToggle.isOn);
            CameraManager.ShowFog(false);

            CameraManager.ShowLayer(TileLayer.Wall, _layerToggleWall.isOn);
            CameraManager.ShowLayer(TileLayer.Dynamic, _layerToggleDynamic.isOn);
            CameraManager.ShowLayer(TileLayer.Logic, _layerToggleLogic.isOn);
            CameraManager.ShowLayer(TileLayer.Floor, _layerToggleFloor.isOn);
            CameraManager.ShowLayer(TileLayer.Static, _layerToggleStatic.isOn);
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
                    _layerToggleFloor.isOn = !_layerToggleFloor.isOn;
                    break;

                case KeyCode.Alpha2:
                    _layerToggleWall.isOn = !_layerToggleWall.isOn;
                    break;

                case KeyCode.Alpha3:
                    _layerToggleStatic.isOn = !_layerToggleStatic.isOn;
                    break;

                case KeyCode.Alpha4:
                    _layerToggleDynamic.isOn = !_layerToggleDynamic.isOn;
                    break;

                case KeyCode.Alpha5:
                    _layerToggleLogic.isOn = !_layerToggleLogic.isOn;
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

        private void ShowCameraEditor(GameCamera gameCamera)
        {
            _cameraEditor.gameCamera = gameCamera;
            _cameraEditor.gameObject.SetActive(true);
        }

        private void HideCameraEditor()
        {
            _cameraEditor.gameObject.SetActive(false);
        }

        public static void ShowTooltip (RectTransform rectTransform, string text, TooltipDirection direction)
        {
            instance._tooltip.Show(rectTransform, text, direction);
        }

        public static void HideTooltip ()
        {
            instance._tooltip.gameObject.SetActive(false);
        }

        /// <summary>
        /// Returns true if the given layer is visible within the editor
        /// </summary>
        /// <param name="layer">Layer</param>
        /// <returns>True if the layer is visible</returns>
        public static bool IsLayerVisible (TileLayer layer)
        {
            switch (layer)
            {
                case TileLayer.Wall: return instance._layerToggleWall.isOn;
                case TileLayer.Dynamic: return instance._layerToggleDynamic.isOn;
                case TileLayer.Static: return instance._layerToggleStatic.isOn;
                case TileLayer.Logic: return instance._layerToggleLogic.isOn;
                case TileLayer.Floor: return instance._layerToggleFloor.isOn;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Create a visibility mask for all layers using the current visibility state
        /// </summary>
        /// <returns>Mask where each layer is represented by a single bit</returns>
        public static uint GetVisibleLayerMask ()
        {
            uint mask = 0;
            for (var layer = TileLayer.Floor; layer <= TileLayer.Logic; layer++)
                mask |= (IsLayerVisible(layer) ? (1u << (int)layer) : 0u);

            return mask;
        }

        public static void Import()
        {
            instance.ShowPopup(instance._importPopup.gameObject);
            instance._importPopup.Import((path) => {
                // Re-add all of the world decals to the palette
                //instance._chooseDecalPalette.RemoveImportedDecals();

                var texture = new Texture2D(1, 1);
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                    return;

                instance._chooseDecalPalette.AddDecal(new Decal(Guid.NewGuid(), texture));
                instance._decalPalette.AddDecal(new Decal(Guid.NewGuid(), texture));
            });
        }

        public void ChooseColor (Color color, RectTransform rectTransform, Action<Color,bool> valueChanged)
        {
            var parentTransform = (_colorPicker.transform.parent as RectTransform);
            var pickerTransform = _colorPicker.transform as RectTransform;
            var bounds = rectTransform.TransformBoundsTo(parentTransform);
            var min = (Vector2)bounds.min - parentTransform.rect.min;
            var max = (Vector2)bounds.max - parentTransform.rect.min;

            if((min.y + max.y) * 0.5f > 0.5f)
            {
                pickerTransform.pivot = Vector2.one;
                pickerTransform.anchorMin = pickerTransform.anchorMax = new Vector2(
                    min.x / parentTransform.rect.size.x,
                    max.y / parentTransform.rect.size.y);
            }
            else
            {
                pickerTransform.pivot = new Vector2(1.0f, 0.0f);
                pickerTransform.anchorMin = pickerTransform.anchorMax = new Vector2(
                    min.x / parentTransform.rect.size.x,
                    min.y / parentTransform.rect.size.y);
            }

            _colorPicker.value = color;
            _colorPicker.onValueChanged = valueChanged;
            ShowPopup(_colorPicker.gameObject);
        }
    }
}
