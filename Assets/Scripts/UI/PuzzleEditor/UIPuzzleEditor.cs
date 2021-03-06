﻿using System;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

using Puzzled.UI;
using System.Collections.Generic;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor : UIScreen, KeyboardManager.IKeyboardHandler
    {
        public const int DefaultZoom = 12;
        public const int MaxZoom = 30;

        public enum Mode
        {
            Unknown,
            Select,
            Draw,
            Erase,
            Decal
        }

        [Header("General")]
        [SerializeField] private UICanvas _canvas = null;
        [SerializeField] private TMPro.TextMeshProUGUI puzzleName = null;
        [SerializeField] private Button _playButton = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private Button _saveButton = null;
        [SerializeField] private Button _closeButton = null;
        [SerializeField] private GameObject dragWirePrefab = null;
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
        [SerializeField] private UIRadio _postProcToggle = null;
        [SerializeField] private UIRadio _debugToggle = null;
        [SerializeField] private GameObject _canvasControls = null;

        [SerializeField] private GameObject _playControls = null;

        [Header("Gizmos")]
        [SerializeField] private GameObject _wireGizmo = null;

        [Header("Toolbar")]
        [SerializeField] private GameObject _toolbar = null;
        [SerializeField] private Button _menuButton = null;
        [SerializeField] private UIRadio _selectTool = null;
        [SerializeField] private UIRadio _drawTool = null;
        [SerializeField] private UIRadio _eraseTool = null;
        [SerializeField] private UIRadio _decalTool = null;
        [SerializeField] private Button _undoButton = null;
        [SerializeField] private Button _redoButton = null;

        [Header("Popups")]
        [SerializeField] private GameObject popups = null;
        [SerializeField] private Image _popupDarken = null;
        [SerializeField] private GameObject _menu = null;
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
        [SerializeField] private UIColorPicker _colorPicker = null;

        [Header("Sprites")]
        public Sprite spriteDecalPower = null;

        private Mode _mode = Mode.Unknown;
        private Puzzle _puzzle = null;
        private World.IPuzzleEntry _puzzleEntry = null;
        private bool playing;
        private Action<Tile> _chooseTileCallback;
        private Action<Decal> _chooseDecalCallback;
        private Action<Background> _chooseBackgroundCallback;
        private Action<Sound> _chooseSoundCallback;
        private Mode savedMode;
        private Action<KeyCode> _onKey;
        private Action<bool, bool, bool> _onKeyModifiers;
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
                        _drawTool.isOn = true;
                        break;
                    case Mode.Select:
                        _selectTool.isOn = true;
                        break;
                    case Mode.Erase:
                        _eraseTool.isOn = true;
                        break;
                    case Mode.Decal:
                        _decalTool.isOn = true;
                        break;
                }

                UpdateMode();
            }
        }

        public static void Initialize(World.IPuzzleEntry puzzleEntry)
        {
            if (null == puzzleEntry)
                return;

            UIManager.loading = true;
            UIManager.HideMenu();
            SceneManager.LoadSceneAsync("Editor", LoadSceneMode.Additive).completed += (handle) => {
                UIManager.loading = false;
                instance.Load(puzzleEntry);
            };
        }

        public static void Shutdown()
        {
            if (instance == null)
                return;

            instance._chooseDecalPalette.UnloadDecals();
            instance._decalPalette.UnloadDecals();

            instance._pointerAction.action.performed -= instance.OnPointerMoved;

            instance.gameObject.SetActive(false);
            SceneManager.UnloadSceneAsync("Editor");

            UIManager.ReturnToEditWorldScreen();

            if(null != instance._puzzleEntry)
                instance._puzzleEntry.world.UnloadAllTextures();
        }

        private void UpdateMode()
        {
            inspector.SetActive(false);

            _getCursor = null;
            _onKey = null;
            _onKeyModifiers = null;

            _canvas.UnregisterAll();

            DisableSelectTool();
            DisableDrawTool();
            DisableEraseTool();
            DisableDecalTool();

            switch (_mode)
            {
                case Mode.Select:
                    EnableSelectTool();
                    break;
                case Mode.Draw:
                    EnableDrawTool();
                    break;
                case Mode.Erase:
                    EnableEraseTool();
                    break;
                case Mode.Decal:
                    EnableDecalTool();
                    break;
            }

            UpdateWireVisibility();

            // Clear selection if the inspector is not enabled
            if (!inspector.gameObject.activeSelf)
            {
                puzzle.HideWires();
                ClearSelection();
            }

            UpdateCursor();
        }

        private void Awake()
        {
            instance = this;

            _inspectorTileName.onEndEdit.AddListener(OnInspectorTileNameChanged);

            _saveButton.onClick.AddListener(() => {
                Save();
                HidePopup();
            });
            _closeButton.onClick.AddListener(() => {
                Save();
                Shutdown();
            });

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

            _postProcToggle.onValueChanged.AddListener((v) => PostProcManager.disableAll = !v); 

            _inspectorRotateButton.onClick.AddListener(() => {
                var rotation = inspectorTile.GetProperty("rotation");
                if (null != rotation)
                    ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(inspectorTile, "rotation", rotation.GetValue<int>(inspectorTile) + 1));
            });            

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

            _undoButton.onClick.AddListener(Undo);
            _redoButton.onClick.AddListener(Redo);
            _playButton.onClick.AddListener(BeginPlay);

            _menuButton.onClick.AddListener(() => ShowPopup(_menu));

            _layerToggleWall.onValueChanged.AddListener((v) => UpdateCameraFlags());
            _layerToggleDynamic.onValueChanged.AddListener((v) => UpdateCameraFlags());
            _layerToggleLogic.onValueChanged.AddListener((v) => UpdateCameraFlags());
            _layerToggleStatic.onValueChanged.AddListener((v) => UpdateCameraFlags());
            _layerToggleFloor.onValueChanged.AddListener((v) => UpdateCameraFlags());

            _selectTool.onValueChanged.AddListener((v) => OnToolChanged());
            _decalTool.onValueChanged.AddListener((v) => OnToolChanged());
            _drawTool.onValueChanged.AddListener((v) => OnToolChanged());
            _eraseTool.onValueChanged.AddListener((v) => OnToolChanged());
        }

        private void OnEnable()
        {
            popups.SetActive(false);

            KeyboardManager.Push(this);

            _canvas.onScroll = OnScroll;
            _canvas.onRButtonDrag = OnPan;

            GameManager.Stop();
            GameManager.busy++;

            inspector.SetActive(false);

            // Start off with an empty puzzle
            //NewPuzzle();

//            mode = Mode.Draw;
            //drawTool.isOn = true;

            ClearUndo();

            InitializeCursor();

            UpdateCameraFlags();

            // Uncomment to convert all files
            // UpgradeAllFiles();
        }

        private void OnScroll(Vector2 position, Vector2 delta)
        {
            if (playing || delta.y == 0 || !_canvas.isMouseOver)
                return;

            UpdateZoom(_cameraZoom + (delta.y > 0 ? -1 : 1));
        }

        private void UpdateZoom(int zoom)
        {
            _cameraZoom = Mathf.Clamp(zoom, CameraManager.MinZoom, MaxZoom);

            UpdateCamera();

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

            if (_drawTool.isOn)
                mode = Mode.Draw;
            else if (_selectTool.isOn)
                mode = Mode.Select;
            else if (_eraseTool.isOn)
                mode = Mode.Erase;
            else if (_decalTool.isOn)
                mode = Mode.Decal;
        }

        private void OnPan(Vector2 position, Vector2 delta)
        {
            // Do not allow panning in play mode
            if (playing)
                return;

            _cameraTarget += -(_canvas.CanvasToWorld(position + delta) - _canvas.CanvasToWorld(position));

            UpdateCamera();
            UpdateCursor();
        }


        public void OnCancelPopup()
        {
            HidePopup();
        }

        public void SaveAs(string filename)
        {
#if false
            _puzzle.Save(filename);
            puzzleName.text = _puzzle.filename;
#endif
        }

        public void Save()
        {
            if (!_puzzle.isModified)
                return;

            _puzzleEntry.Save(_puzzle);

            // Add a star to the end of the puzzle name
            if (instance.puzzleName.text.EndsWith("*"))
                instance.puzzleName.text = instance.puzzleName.text.Substring(0, instance.puzzleName.text.Length - 1);

            HidePopup();
        }

        public static void Stop() => instance.OnStopButton();


        public void OnStopButton() => EndPlay();
        public void OnDebugButton()
        {
            if (!playing)
                return;

            GameManager.puzzle.ShowWires(isDebugging);
            CameraManager.ShowWires(isDebugging);
            CameraManager.ShowLayer(TileLayer.Logic, isDebugging);
            CameraManager.ShowLayer(TileLayer.InvisibleStatic, isDebugging);
        }

        private void BeginPlay()
        {
            // Do not allow playing if already playing
            if (playing)
                return;

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
            _canvas.gameObject.SetActive(false);

            GameManager.busy = 0;

            _playButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

            playing = true;

            ClearSelection();

            // Load the puzzle and play
            GameManager.LoadPuzzle(_puzzleEntry);
            GameManager.Play();

            if (isDebugging)
                OnDebugButton();

            PostProcManager.disableAll = false;
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

            // Always return to the select tool when exiting play mode
            inspector.SetActive(mode == Mode.Select);

            playing = false;
            _playButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            _canvasControls.SetActive(true);
            _canvas.gameObject.SetActive(true);

            // Set our editing puzzle as active
            Puzzle.current = _puzzle;

            _puzzle.StartAllTiles();

            // Return to the saved mode
            mode = savedMode;

            LightmapManager.Render();

            UpdateCameraFlags();

            PostProcManager.disableAll = !_postProcToggle.isOn;
        }

        private void UpdateCamera()
        {
            CameraManager.editorCameraState = new GameCamera.State
            {
                targetPosition = _cameraTarget,
                pitch = _pitchSlider.value, 
                yaw = _yawSlider.value * 90,
                yawIndex = 0,
                zoomLevel = _cameraZoom
            };

            CameraManager.ForceUpdate();
        }

        private void Center(Tile[] tiles, int zoom = -1)
        {
            if (tiles == null || tiles.Length == 0)
                return;

            var bounds = puzzle.grid.CellToWorldBounds(tiles[0].cell);
            foreach(var tile in tiles)
                bounds.Encapsulate(puzzle.grid.CellToWorldBounds(tile.cell));

            _cameraTarget = bounds.center;
            _cameraZoom = zoom == -1 ? _cameraZoom : zoom;
            UpdateCamera();
        }

        private void Center(Cell cell, int zoom = -1)
        {
            _cameraTarget = _puzzle.grid.CellToWorld(cell);
            _cameraZoom = zoom == -1 ? _cameraZoom : zoom;

            UpdateCamera();
        }

        public void Load(World.IPuzzleEntry puzzleEntry)
        {
            try
            {
                if (_puzzle != null)
                    _puzzle.Destroy();

                _puzzleEntry = puzzleEntry;
                _puzzle = puzzleEntry.Load();
                _puzzle.isEditing = true;
                _puzzle.showGrid = _gridToggle.isOn;
                _puzzle.name = puzzleEntry.name;
                Puzzle.current = _puzzle;

                // Always start in the select tool
                mode = Mode.Select;

                ClearSelection();

                puzzleName.text = _puzzle.name;

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

                _puzzleEntry.world.LoadAllTextures();

                // Add the world decals to the decal palette.
                instance._chooseDecalPalette.LoadDecals(_puzzleEntry.world);
                instance._decalPalette.LoadDecals(_puzzleEntry.world);

                LightmapManager.Render();
            } 
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            HidePopup();
            UpdateMode();
        }

        private void ShowPopup(GameObject popup, bool darken=true)
        {
            if (popup.transform.parent != popups.transform)
                return;

            _popupDarken.enabled = darken;
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
        public Tile GetTopMostTileWithProperty(Cell cell, string propertyName, TileLayer topLayer = TileLayer.InvisibleStatic)
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

        /// <summary>
        /// Returns the top most visible tile in the given cell that contains a property of the given type
        /// </summary>
        /// <param name="cell">Cell to search</param>
        /// <param name="propertyType">Name of property to search for</param>
        /// <param name="topLayer">Layer to start searching from</param>
        /// <returns>Topmost tile in the cell with the given property or null</returns>
        public Tile GetTopMostTileWithPropertyType(Cell cell, TilePropertyType propertyType, TileLayer topLayer = TileLayer.InvisibleStatic)
        {
            for (int i = (int)topLayer; i >= 0; i--)
            {
                var tile = _puzzle.grid.CellToTile(cell, (TileLayer)i);
                if (null == tile)
                    continue;

                // Do not return tiles on hidden layers
                if (!IsLayerVisible((TileLayer)i))
                    continue;

                if (!tile.properties.Any(p => p.type == propertyType))
                    continue;

                return tile;
            }

            return null;
        }

        private Tile GetTopMostTile(Cell cell, TileLayer topLayer = TileLayer.InvisibleStatic)
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

            var nextTile = GetTopMostTile(cell, layer != TileLayer.Floor ? (layer - 1) : TileLayer.InvisibleStatic);
            if (null == nextTile)
            {
                switch (cell.system)
                {
                    case CellCoordinateSystem.Edge:
                        return GetNextTile(cell, TileLayer.InvisibleStatic, CellCoordinateSystem.SharedEdge);

                    case CellCoordinateSystem.SharedEdge:
                        return GetNextTile(cell, TileLayer.InvisibleStatic, CellCoordinateSystem.Grid);

                    default:
                        nextTile = GetTopMostTile(cell, TileLayer.InvisibleStatic);
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
            CameraManager.ShowLayer(TileLayer.InvisibleStatic, _layerToggleStatic.isOn);
        }

        void KeyboardManager.IKeyboardHandler.OnKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Escape:
                    // If the select tool is performing a drag operation then cancel it
                    if (isSelectToolDragging)
                    {
                        CancelDrag();
                        return;
                    }

                    if (playing)
                        OnStopButton();
                    else
                        ShowPopup(_menu);
                    break;
            }

            if (playing)
                return;

            switch (keyCode)
            {
                case KeyCode.Space:
                    BeginPlay();
                    break;

                case KeyCode.V:
                    mode = Mode.Select;
                    break;

                case KeyCode.B:
                    mode = Mode.Draw;
                    break;

                case KeyCode.E:
                    mode = Mode.Erase;
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
            _onKeyModifiers?.Invoke(shift, ctrl, alt);

            UpdateCursor();
        }

#if false
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
#endif

        private void ShowCameraEditor(GameCamera gameCamera)
        {
            _cameraEditor.gameCamera = gameCamera;
            _cameraEditor.gameObject.SetActive(true);
        }

        private void HideCameraEditor()
        {
            _cameraEditor.gameObject.SetActive(false);
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
            for (var layer = TileLayer.Floor; layer <= TileLayer.InvisibleStatic; layer++)
                mask |= (IsLayerVisible(layer) ? (1u << (int)layer) : 0u);

            return mask;
        }

        /// <summary>
        /// Import a new custom decal
        /// </summary>
        /// <param name="callback">Callback to call with newly imported decal, or empty decal if none was imported</param>
        public static void Import(Action<Decal> callback)
        {
            var ofn = new OpenFileDialog.OpenFileName();
            ofn.filter = "All Files\0*.*\0Images\0*.png\0\0";
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = Application.dataPath;
            ofn.title = "Upload Image";
            ofn.defExt = "PNG";
            ofn.flags = OpenFileDialog.OFN_EXPLORER | 
                OpenFileDialog.OFN_FILEMUSTEXIST | 
                OpenFileDialog.OFN_PATHMUSTEXIST | 
                OpenFileDialog.OFN_ALLOWMULTISELECT | 
                OpenFileDialog.OFN_NOCHANGEDIR;

            // Empty keyboard handler to make sure no keys are processed during this time
            KeyboardManager.Push(null);
            if (OpenFileDialog.GetOpenFileName(ofn))
            {
                var path = ofn.file;
                Debug.Log(path);
                if (null == path)
                {
                    callback?.Invoke(Decal.none);
                    return;
                }

                var guid = instance._puzzleEntry.world.AddTexture(path, Path.GetFileName(path));
                if (guid == Guid.Empty)
                    return;

                var decal = instance._puzzleEntry.world.GetDecal(guid);
                instance._chooseDecalPalette.AddDecal(decal);
                instance._decalPalette.AddDecal(decal);

                callback?.Invoke(decal);
            } else
                callback?.Invoke(Decal.none);

            // Force the input system to process any keys before popping the emoty keyboard handler off
            UnityEngine.InputSystem.InputSystem.Update();
            KeyboardManager.Pop();
        }

        public void ChooseColor (Color color, RectTransform rectTransform, Action<Color,bool> valueChanged)
        {
            var parentTransform = (_colorPicker.transform as RectTransform);
            var pickerTransform = _colorPicker.popup.transform as RectTransform;
            var bounds = rectTransform.TransformBoundsTo(parentTransform);
            var min = (Vector2)bounds.min - parentTransform.rect.min;
            var max = (Vector2)bounds.max - parentTransform.rect.min;

            if((min.y + max.y) * 0.5f / parentTransform.rect.size.y > 0.5f)
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

            ShowPopup(_colorPicker.gameObject, false);
            _colorPicker.value = color;
            _colorPicker.onValueChanged = valueChanged;
        }
    }
}
