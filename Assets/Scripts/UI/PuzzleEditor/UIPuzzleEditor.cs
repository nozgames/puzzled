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
            Draw,
            Select,
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

        [SerializeField] private Camera previewCamera = null;
        [SerializeField] private Transform previewParent = null;
        [SerializeField] private GameObject piecePrefab = null;
        [SerializeField] private Transform pieces = null;
        [SerializeField] private RectTransform selectionRect = null;
        [SerializeField] private TMPro.TextMeshProUGUI puzzleName = null;
        [SerializeField] private Button playButton = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private GameObject dragWirePrefab = null;
        [SerializeField] private GameObject palette = null;
        [SerializeField] private int minZoom = 1;
        [SerializeField] private int maxZoom = 10;
        [SerializeField] private GameObject fileButton = null;

        [SerializeField] private Transform options = null;
        [SerializeField] private GameObject inspector = null;

        [Header("Tools")]
        [SerializeField] private Toggle selectTool = null;
        [SerializeField] private Toggle drawTool = null;
        [SerializeField] private Toggle eraseTool = null;
        [SerializeField] private Toggle wireTool = null;

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

        [Header("Tiles")]
        [SerializeField] private Tile floorTile = null;

        [Header("Options")]
        [SerializeField] private UIOptionEditor optionPrefabInt = null;
        [SerializeField] private UIOptionEditor optionPrefabBool = null;
        [SerializeField] private UIOptionEditor optionPrefabString = null;
        [SerializeField] private UIOptionEditor optionPrefabTile = null;

        private Mode _mode = Mode.Draw;
        private string currentPuzzleName = null;
        private string puzzleToLoad = null;
        private WireMesh dragWire = null;

        private RectInt selection;
        private Tile drawTile = null;
        private Vector2Int dragStart;
        private Vector2Int dragEnd;
        private bool dragging;
        private bool ignoreMouseUp;
        private bool playing;
        private bool panning;
        private Vector3 panPointerStart;

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
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            GameManager.Stop();
            GameManager.IncBusy();
            GameManager.onTileInstantiated += OnTileInstantiated;

            popups.gameObject.SetActive(false);
            inspector.SetActive(false);

            pieces.transform.DetachAndDestroyChildren();

            foreach(var tile in TileDatabase.GetTiles())
                GeneratePreview(tile);

            previewParent.DetachAndDestroyChildren();

            pointerAction.action.performed += OnPointerMoved;
            pointerDownAction.action.performed += OnPointerDown;
            rightClickAction.action.performed += OnRightClick;
            middleClickAction.action.performed += OnMiddleClick;
            mouseWheelAction.action.performed += OnMouseWheel;

            selectionRect.gameObject.SetActive(false);

            drawTool.isOn = true;

            GameManager.Instance.ClearTiles();
            currentPuzzleName = null;
            puzzleName.text = "Unnamed";
        }

        private void OnTileInstantiated(Tile tile, Tile prefab)
        {
            var editorInfo = tile.gameObject.AddComponent<TileEditorInfo>();
            editorInfo.guid = TileDatabase.GetGuid(prefab);
        }

        private void OnRightClick(InputAction.CallbackContext ctx)
        {
            switch (mode)
            {
                case Mode.Wire:
                {
                    var wire = HitTestWire(pointerWorld);
                    if(null != wire)
                        RemoveWire(wire);
                    break;
                }
            }
        }

        private void OnMiddleClick(InputAction.CallbackContext ctx)
        {
            panning = ctx.ReadValueAsButton();
            if(panning)
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
            GameManager.onTileInstantiated -= OnTileInstantiated;

            if (GameManager.Instance != null)
                GameManager.DecBusy();

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

            if (previewParent.childCount > 0)
                previewParent.GetChild(0).gameObject.SetActive(false);

            previewParent.DetachAndDestroyChildren();
            
            var blockObject = Instantiate(prefab.gameObject, previewParent);
            blockObject.SetChildLayers(LayerMask.NameToLayer("Preview"));

            previewCamera.Render();

            var t = new Texture2D(previewCamera.targetTexture.width, previewCamera.targetTexture.height, TextureFormat.ARGB32, false);
            t.filterMode = FilterMode.Point;

            RenderTexture.active = previewCamera.targetTexture;
            t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
            t.Apply();            

            var tileObject = Instantiate(piecePrefab, pieces);
            var toggle = tileObject.GetComponent<Toggle>();
            toggle.group = pieces.GetComponent<ToggleGroup>();
            toggle.onValueChanged.AddListener(v => {
                if(v)
                    drawTile = prefab;
            });
            tileObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = prefab.info.displayName;
            tileObject.GetComponentInChildren<RawImage>().texture = t;            
        }

        public void OnToolChanged()
        {
            if (null == GameManager.Instance)
                return;

            if (drawTool.isOn)
                mode = Mode.Draw;
            else if (selectTool.isOn)
                mode = Mode.Select;
            else if (eraseTool.isOn)
                mode = Mode.Erase;
            else if (wireTool.isOn)
                mode = Mode.Wire;

            GameManager.Instance.ShowWires(wireTool.isOn);
            inspector.SetActive(wireTool.isOn);
            palette.SetActive(mode == Mode.Draw);
        }

        private void HandleDrag(DragState state, Vector2Int cell)
        {
            switch (mode)
            {
                case Mode.Draw:
                    if (null == drawTile)
                        return;

                    if (drawTile.info.layer == TileLayer.Dynamic && GameManager.Instance.HasCellTiles(cell))
                        foreach (var actor in GameManager.Instance.GetCellTiles(cell))
                            if (!actor.info.allowDynamic)
                                return;

                    GameManager.Instance.ClearTile(cell, drawTile.info.layer);

                    // Destroy all other instances of this tile regardless of variant
                    if (!drawTile.info.allowMultiple)
                    {
                        foreach (var actor in GameManager.GetTiles().Where(a => a.info == drawTile.info))
                        {
                            Destroy(actor.gameObject);

                            // Replace the static actor with a floor so we dont leave a hole
                            if (drawTile.info.layer == TileLayer.Static)
                                GameManager.InstantiateTile(floorTile, actor.cell);
                        }
                    }

                    // Automatically add floor
                    if (drawTile.info.layer == TileLayer.Dynamic)
                        GameManager.InstantiateTile(floorTile, cell);

                    GameManager.InstantiateTile(drawTile, cell);
                    break;

                case Mode.Erase:
                    GameManager.Instance.ClearTile(cell);
                    break;

                case Mode.Select:
                    if (state == DragState.Begin)
                        SelectTile(dragStart);

                    SetSelectionRect(dragStart, dragEnd);
                    break;

                case Mode.Wire:
                    switch (state)
                    {
                        case DragState.Begin:
                        {
                            var tile = GetTile(cell);
                            if (tile != null)
                            {
                                GameManager.Instance.HideWires();
                                GameManager.Instance.ShowWires(tile);
                                SetSelectionRect(cell, cell);
                                SelectTile(cell);
                            } 
                            else
                            {
                                GameManager.Instance.ShowWires(true);
                            }
                            break;
                        }

                        case DragState.Update:
                        {
                            if(null == dragWire && dragStart != dragEnd && hasSelection && GetTile(dragStart).info.allowWireOutputs)
                            {
                                dragWire = Instantiate(dragWirePrefab).GetComponent<WireMesh>();
                                dragWire.transform.position = GameManager.CellToWorld(dragStart);
                                dragWire.target = cell;
                            }
                            else if(dragWire != null)
                            {
                                dragWire.target = cell;
                            }
                            break;
                        }

                        case DragState.End:
                        {
                            if(dragWire != null)
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

        private void SetSelectionRect(Vector2Int min, Vector2Int max)
        {
            var anchorCell = Vector2Int.Min(min, max);
            var size = Vector2Int.Max(min, max) - anchorCell;

            selectionRect.anchorMin = Camera.main.WorldToViewportPoint (GameManager.CellToWorld(anchorCell) - new Vector3(0.5f, 0.5f, 0));
            selectionRect.anchorMax = Camera.main.WorldToViewportPoint(GameManager.CellToWorld(anchorCell+size) + new Vector3(0.5f, 0.5f, 0));

            selectionRect.gameObject.SetActive(true);
        }

        private void SelectTile(Vector2Int cell) => SelectTile(GetTile(cell));

        private void SelectTile (Tile tile)
        {
            if (tile == null)
            {                
                options.DetachAndDestroyChildren();
                return;
            }

            PopulateOptions(tile);
        }

        private Vector2Int pointerCell;
        private Vector2 pointer;
        private Vector3 pointerWorld;

        private void OnPointerMoved(InputAction.CallbackContext ctx)
        {
            pointer = ctx.ReadValue<Vector2>();
            pointerWorld = Camera.main.ScreenToWorldPoint(pointer);
            var cell = GameManager.WorldToCell(pointerWorld + new Vector3(0.5f, 0.5f, 0));
            if (cell != pointerCell)
                pointerCell = cell;

            if(panning)
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

            if(!mouseDown && ignoreMouseUp)
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
            }
            else if (dragging)
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
            Puzzle.Save(GameManager.Instance.transform.GetChild(1), currentPuzzleFilename);
        }

        private void NewPuzzle ()
        {
            selectionRect.gameObject.SetActive(false);
            currentPuzzleName = null;
            puzzleName.text = "Unnamed";
            GameManager.PanCenter();
            GameManager.Instance.ClearTiles();
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
            foreach(var file in files)
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
            if(null != puzzleToLoad)
                Load(puzzleToLoad);
        }

        public void OnStopButton()
        {
            if (!playing)
                return;

            GameManager.Stop();

            GameManager.ClearBusy();
            GameManager.IncBusy();

            palette.SetActive(true);
            selectTool.gameObject.SetActive(true);
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

            if(!hasPuzzleName)
            {
                ShowPopup(puzzleNamePopup);
                return;
            }

            palette.SetActive(false);
            selectTool.gameObject.SetActive(false);
            drawTool.gameObject.SetActive(false);
            eraseTool.gameObject.SetActive(false);
            wireTool.gameObject.SetActive(false);
            fileButton.gameObject.SetActive(false);

            GameManager.ClearBusy();

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
        }

        public void OnFileMenuButton()
        {
            ShowPopup(fileMenuPopup);
        }

        private void ShowPopup (GameObject popup)
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

        private Tile GetTile (Vector2Int cell)
        {
            var tiles = GameManager.Instance.GetCellTiles(cell);
            if (tiles == null || tiles.Count == 0)
                return null;

            return tiles.OrderByDescending(t => t.info.layer).FirstOrDefault();
        }

        private void RemoveWire(Wire wire)
        {
            Destroy(wire.gameObject);
        }

        private Wire HitTestWire (Vector3 pointer)
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
            var editorInfo = tile.GetComponent<TileEditorInfo>();
            if (null == editorInfo.editableProperties)
                return;

            options.DetachAndDestroyChildren();

            if (tile.info.optionEditors != null)
                foreach (var optionEditorInfo in tile.info.optionEditors)
                    Instantiate(optionEditorInfo.prefab, options).GetComponent<UIOptionEditor>().target = tile;

            foreach (var editableProperty in editorInfo.editableProperties)
            {
                var optionEditor = InstantiateOptionEditor(editableProperty.property.PropertyType);
                if (null == optionEditor)
                    continue;

                optionEditor.target = editableProperty;
            }
        }

        public void OpenTileSelector (Action<Tile> callback)
        {
            ShowPopup(tileSelectorPopup);

            tileSelectorTiles.DetachAndDestroyChildren();

            for (int i = 0; i < palette.transform.childCount; i++)
                Instantiate(palette.transform.GetChild(i).gameObject, tileSelectorTiles);
        }
    }
}
