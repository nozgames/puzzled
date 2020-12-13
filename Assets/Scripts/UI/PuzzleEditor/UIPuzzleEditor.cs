using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using Puzzled.PuzzleEditor;

namespace Puzzled
{
    public class UIPuzzleEditor : UIScreen
    {
        private enum Mode
        {
            Unknown,
            Draw,
            Move,
            Erase,
            Wire
        }

        private enum DragState
        {
            Begin,
            Update,
            End
        }

        [SerializeField] private GameObject piecePrefab = null;
        [SerializeField] private RectTransform selectionRect = null;
        [SerializeField] private TMPro.TextMeshProUGUI puzzleName = null;
        [SerializeField] private Button playButton = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private GameObject dragWirePrefab = null;
        [SerializeField] private Transform paletteTiles = null;
        [SerializeField] private GameObject palette = null;
        [SerializeField] private int minZoom = 1;
        [SerializeField] private int maxZoom = 10;
        [SerializeField] private GameObject fileButton = null;
        [SerializeField] private GameObject tileButtonPrefab = null;

        [SerializeField] private Transform options = null;
        [SerializeField] private GameObject inspector = null;

        [Header("Tools")]
        [SerializeField] private Toggle moveTool = null;
        [SerializeField] private Toggle drawTool = null;
        [SerializeField] private Toggle eraseTool = null;
        [SerializeField] private Toggle wireTool = null;
        [SerializeField] private GameObject moveToolOptions = null;
        [SerializeField] private GameObject eraseToolOptions = null;
        [SerializeField] private Toggle eraseToolAllLayers = null;

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

        [Header("Input")]
        [SerializeField] private InputActionReference pointerAction;
        [SerializeField] private InputActionReference pointerDownAction;
        [SerializeField] private InputActionReference rightClickAction;
        [SerializeField] private InputActionReference middleClickAction;
        [SerializeField] private InputActionReference mouseWheelAction;

        [Header("Inspector")]
        [SerializeField] private GameObject inspectorContent = null;
        [SerializeField] private TMPro.TMP_InputField inspectorTileName = null;
        [SerializeField] private RawImage inspectorTilePreview = null;
        [SerializeField] private UIOptionEditor optionPrefabInt = null;
        [SerializeField] private UIOptionEditor optionPrefabBool = null;
        [SerializeField] private UIOptionEditor optionPrefabString = null;
        [SerializeField] private UIOptionEditor optionPrefabTile = null;


        private Mode _mode = Mode.Unknown;
        private string currentPuzzleName = null;
        private string puzzleToLoad = null;
        private WireMesh dragWire = null;

        //private RectInt selection;

        private Tile selection = null;
        private Tile drawTile = null;
        private Cell dragStart;
        private Cell dragEnd;
        private bool dragging;
        private bool ignoreMouseUp;
        private bool playing;
        private bool panning;
        private Vector3 panPointerStart;
        private Action<Tile> tileSelectorCallback;

        private bool hasSelection => selectionRect.gameObject.activeSelf;
        private bool hasPuzzleName => !string.IsNullOrEmpty(currentPuzzleName);

        public static UIPuzzleEditor instance { get; private set; }

        private Mode mode {
            get => _mode;
            set {
                if (_mode == value)
                    return;

                selectionRect.gameObject.SetActive(false);
                SelectTile(null);

                _mode = value;

                if (_mode == Mode.Wire)
                    SelectTile(null);

                inspector.SetActive(_mode == Mode.Wire);
                palette.SetActive(mode == Mode.Draw);
                moveToolOptions.SetActive(mode == Mode.Move);
                eraseToolOptions.SetActive(mode == Mode.Erase);
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            GameManager.Stop();
            GameManager.busy++;

            popups.gameObject.SetActive(false);
            inspector.SetActive(false);

            paletteTiles.transform.DetachAndDestroyChildren();

            foreach (var tile in TileDatabase.GetTiles())
                GeneratePreview(tile);

            pointerAction.action.performed += OnPointerMoved;
            pointerDownAction.action.performed += OnPointerDown;
            rightClickAction.action.performed += OnRightClick;
            middleClickAction.action.performed += OnMiddleClick;
            mouseWheelAction.action.performed += OnMouseWheel;

            selectionRect.gameObject.SetActive(false);

            mode = Mode.Draw;
            drawTool.isOn = true;

            GameManager.UnloadPuzzle();
            currentPuzzleName = null;
            puzzleName.text = "Unnamed";
        }

        private void OnRightClick(InputAction.CallbackContext ctx)
        {
            switch (mode)
            {
                case Mode.Wire:
                {
                    var wire = HitTestWire(pointerWorld);
                    if (null != wire)
                        RemoveWire(wire);
                    break;
                }
            }
        }

        private void OnMiddleClick(InputAction.CallbackContext ctx)
        {
            panning = ctx.ReadValueAsButton();
            if (panning)
            {
                panPointerStart = pointerWorld;
            }
        }

        private void OnMouseWheel(InputAction.CallbackContext ctx)
        {
            var value = ctx.ReadValue<Vector2>();
            if (value.y == 0)
                return;

            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + (value.y > 0 ? -1 : 1), minZoom, maxZoom);

            if (hasSelection)
                SetSelectionRect(dragStart, dragEnd);
        }

        private void OnDisable()
        {
            GameManager.busy--;

            pointerAction.action.performed -= OnPointerMoved;
            pointerDownAction.action.performed -= OnPointerDown;
            rightClickAction.action.performed -= OnRightClick;
            middleClickAction.action.performed -= OnMiddleClick;
            mouseWheelAction.action.performed -= OnMouseWheel;
        }

        private void GeneratePreview(Tile prefab)
        {
            if (null == prefab)
                return;

            var t = TileDatabase.GetPreview(prefab.guid);

            var tileObject = Instantiate(piecePrefab, paletteTiles);
            var toggle = tileObject.GetComponent<Toggle>();
            toggle.group = paletteTiles.GetComponent<ToggleGroup>();
            toggle.onValueChanged.AddListener(v => {
                if (v)
                    drawTile = prefab;
            });
            tileObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = prefab.info.displayName;
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
                mode = Mode.Wire;
        }

        private void HandleDrag(DragState state, Cell cell)
        {
            switch (mode)
            {
                case Mode.Draw:
                    if (null == drawTile || state == DragState.End)
                        return;

                    // Dont draw if the same tile is already there.  This will prevent
                    // accidental removal of connections and properties
                    var existing = TileGrid.CellToTile(cell, drawTile.info.layer);
                    if (existing != null && existing.guid == drawTile.guid)
                        return;

                    // Static objects cannot be placed on floor objects.
                    if (drawTile.info.layer == TileLayer.Dynamic)
                    {
                        var staticTile = TileGrid.CellToTile(cell, TileLayer.Static);
                        if (staticTile != null && !staticTile.info.allowDynamic)
                            return;
                    }

                    // Remove what is already in that slot
                    // TODO: if it is just a variant we should be able to swap it and reapply the connections and properties
                    TileGrid.UnlinkTile(cell, drawTile.info.layer, true);

                    // Destroy all other instances of this tile regardless of variant
                    if (!drawTile.info.allowMultiple)
                        TileGrid.UnlinkTile(TileGrid.GetLinkedTile(drawTile.info), true);
                    
                    // Create the new tile
                    GameManager.InstantiateTile(drawTile, cell);
                    break;

                case Mode.Erase:
                    if (state == DragState.End)
                        return;

                    if (eraseToolAllLayers.isOn)
                        TileGrid.UnlinkTiles(cell, true);
                    else
                        TileGrid.UnlinkTile(GetTile(cell), true);
                    break;

                case Mode.Move:
                    if (state == DragState.Begin)
                    {
                        SelectTile(dragStart);
                    }

                    SetSelectionRect(dragStart, dragEnd);
                    break;

                case Mode.Wire:
                    switch (state)
                    {
                        case DragState.Begin:
                        {
                            var tile = GetTile(cell, (selection != null && selection.info.layer != TileLayer.Floor && selection.cell == cell) ? ((TileLayer)selection.info.layer - 1) : TileLayer.Logic);
                            if (tile != null)
                            {                                
                                SelectTile(tile);
                            } 
                            else
                            {
                                SelectTile(null);                                
                            }
                            break;
                        }

                        case DragState.Update:
                        {
                            if (null == dragWire && dragStart != dragEnd && hasSelection && GetTile(dragStart).info.allowWireOutputs)
                            {
                                dragWire = Instantiate(dragWirePrefab).GetComponent<WireMesh>();
                                dragWire.transform.position = TileGrid.CellToWorld(dragStart);
                                dragWire.target = cell;
                            } else if (dragWire != null)
                            {
                                dragWire.target = cell;
                            }
                            break;
                        }

                        case DragState.End:
                        {
                            if (dragWire != null)
                            {
                                var wire = GameManager.InstantiateWire(GetTile(dragStart), GetTile(dragEnd));
                                if (wire != null)
                                    wire.visible = true;
                                Destroy(dragWire.gameObject);
                                dragWire = null;
                            }
                            break;
                        }
                    }
                    break;
            }
        }

        private void SetSelectionRect(Cell min, Cell max)
        {
            var anchorCell = Cell.Min(min, max);
            var size = Cell.Max(min, max) - anchorCell;

            selectionRect.anchorMin = Camera.main.WorldToViewportPoint(TileGrid.CellToWorld(anchorCell) - new Vector3(0.5f, 0.5f, 0));
            selectionRect.anchorMax = Camera.main.WorldToViewportPoint(TileGrid.CellToWorld(anchorCell + size) + new Vector3(0.5f, 0.5f, 0));

            selectionRect.gameObject.SetActive(true);
        }

        private void SelectTile(Cell cell) => SelectTile(GetTile(cell));

        private void SelectTile(Tile tile)
        {
            selection = tile;

            if (tile == null)
            {
                selectionRect.gameObject.SetActive(false);                
                options.DetachAndDestroyChildren();
                inspectorContent.SetActive(false);

                GameManager.ShowWires(mode == Mode.Wire);
            }
            else
            {
                Debug.Assert(mode == Mode.Wire);

                inspectorContent.SetActive(true);
                inspectorTileName.text = tile.info.displayName;
                inspectorTilePreview.texture = TileDatabase.GetPreview(tile.guid);
                SetSelectionRect(tile.cell, tile.cell);
                GameManager.HideWires();
                GameManager.ShowWires(tile);
                PopulateOptions(tile);
            }
        }

        private Cell pointerCell;
        private Vector2 pointer;
        private Vector3 pointerWorld;

        private void OnPointerMoved(InputAction.CallbackContext ctx)
        {
            pointer = ctx.ReadValue<Vector2>();
            pointerWorld = Camera.main.ScreenToWorldPoint(pointer);
            var cell = TileGrid.WorldToCell(pointerWorld + new Vector3(0.5f, 0.5f, 0));
            if (cell != pointerCell)
                pointerCell = cell;

            if (panning)
            {
                var delta = pointerWorld - panPointerStart;
                if (delta.magnitude > 0.01f)
                {
                    GameManager.Pan(delta);
                    panPointerStart = pointerWorld;
                }

                if (hasSelection)
                    SetSelectionRect(dragStart, dragEnd);

                return;
            }

            if (dragging && dragEnd != pointerCell)
            {
                dragEnd = pointerCell;
                HandleDrag(DragState.Update, dragEnd);
            }
        }

        private void OnPointerDown(InputAction.CallbackContext ctx)
        {
            var mouseDown = ctx.ReadValueAsButton();

            if (!mouseDown && ignoreMouseUp)
            {
                ignoreMouseUp = false;
                return;
            }

            var results = new List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = pointerAction.action.ReadValue<Vector2>() }, results);
            if (mouseDown && results.Count > 0 && results[0].gameObject == popups)
            {
                ignoreMouseUp = true;
                HidePopup();
                return;
            }

            if (mouseDown && (results.Count <= 0 || results[0].gameObject.GetComponent<UICanvas>() == null))
                return;

            if (mouseDown)
            {
                dragging = true;
                dragStart = pointerCell;
                dragEnd = pointerCell;
                HandleDrag(DragState.Begin, pointerCell);
            } else if (dragging)
            {
                dragging = false;
                dragEnd = pointerCell;
                HandleDrag(DragState.End, pointerCell);
            }
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

        private string currentPuzzleFilename => Path.Combine(Application.dataPath, $"Puzzles/{puzzleName.text}.puzzle");

        public void Save()
        {
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
            HidePopup();
        }

        public void OnNewButton()
        {
            NewPuzzle();
        }

        public void OnLoadButton()
        {
            loadPopupFiles.DetachAndDestroyChildren();

            var files = Directory.GetFiles(System.IO.Path.Combine(Application.dataPath, "Puzzles"), "*.puzzle");
            foreach (var file in files)
            {
                var fileGameObject = Instantiate(loadPopupFilePrefab, loadPopupFiles);
                fileGameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(file);
                var button = fileGameObject.GetComponent<Button>();
                button.onClick.AddListener(() => {
                    puzzleToLoad = Path.GetFileNameWithoutExtension(file);
                    button.Select();
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

            inspector.SetActive(mode == Mode.Wire);
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

            SelectTile(null);
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

            // Ensure no tile is selected
            SelectTile(null);
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
                if (null != tile)
                    return tile;
            }

            // TODO: handle hidden layers

            return null;
        }

        private void RemoveWire(Wire wire)
        {
            Destroy(wire.gameObject);
        }

        private Wire HitTestWire(Vector3 pointer)
        {
            return GameManager.HitTestWire(pointer);
        }

        private UIOptionEditor InstantiateOptionEditor(Type type)
        {
            if (type == typeof(int))
                return Instantiate(optionPrefabInt, options).GetComponent<UIOptionEditor>();
            else if (type == typeof(bool))
                return Instantiate(optionPrefabBool, options).GetComponent<UIOptionEditor>();
            else if (type == typeof(string))
                return Instantiate(optionPrefabString, options).GetComponent<UIOptionEditor>();
            else if (type == typeof(Guid))
                return Instantiate(optionPrefabTile, options).GetComponent<UIOptionEditor>();

            return null;
        }

        private void PopulateOptions(Tile tile)
        {
            options.DetachAndDestroyChildren();

            if (tile.info.optionEditors != null)
                foreach (var optionEditorInfo in tile.info.optionEditors)
                    Instantiate(optionEditorInfo.prefab, options).GetComponent<UIOptionEditor>().target = tile;

            foreach (var tileProperty in tile.properties)
            {
                var optionEditor = InstantiateOptionEditor(tileProperty.property.PropertyType);
                if (null == optionEditor)
                    continue;

                optionEditor.target = new TilePropertyOption(tile, tileProperty);
            }
        }

        public void OpenTileSelector(Type componentType, Action<Tile> callback)
        {
            tileSelectorCallback = callback;

            ShowPopup(tileSelectorPopup);

            tileSelectorTiles.DetachAndDestroyChildren();

            foreach(var tile in TileDatabase.GetTiles().Where(t => t.GetComponent(componentType) != null))
            {
                var tileButton = Instantiate(tileButtonPrefab, tileSelectorTiles).GetComponent<UITileButton>();
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
    }
}
