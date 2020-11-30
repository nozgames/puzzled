using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using Puzzled.PuzzleEditor;
using UnityEditor;

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

        [Serializable]
        private class Block
        {
            public string name;
            public GameObject prefab;
            
            [NonSerialized]
            public Texture2D preview;
        }

        [Serializable]
        private class BlockGroup
        {
            public string name;
            public Sprite icon;
            public Block[] blocks;
        }

        [SerializeField] private TileDatabase tileDatabase = null;
        [SerializeField] private Camera previewCamera = null;
        [SerializeField] private Transform previewParent = null;
        [SerializeField] private GameObject piecePrefab = null;
        [SerializeField] private Transform pieces = null;
        [SerializeField] private RectTransform selectionRect = null;
        [SerializeField] private TMPro.TextMeshProUGUI puzzleName = null;
        [SerializeField] private Button playButton = null;
        [SerializeField] private Button stopButton = null;
        [SerializeField] private GameObject dragWirePrefab = null;

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
        [SerializeField] private TMPro.TMP_InputField puzzleNameInput = null;
        [SerializeField] private Transform loadPopupFiles = null;
        [SerializeField] private GameObject loadPopupFilePrefab = null;

        [Header("Input")]
        [SerializeField] private InputActionReference pointerAction;
        [SerializeField] private InputActionReference pointerDownAction;

        [Header("Tiles")]
        [SerializeField] private Tile floorTile = null;
        [SerializeField] private BlockGroup[] blockGroups;

        private Mode _mode = Mode.Draw;
        private string selectedPuzzle = null;
        private LineRenderer dragWire = null;

        private RectInt selection;
        private Tile selectedTile = null;
        private Vector2Int dragStart;
        private Vector2Int dragEnd;
        private bool dragging;
        private bool ignoreMouseUp;
        private bool playing;

        private bool hasSelection => selectionRect.gameObject.activeSelf;

        private Mode mode {
            get => _mode;
            set {
                if (_mode == value)
                    return;

                selectionRect.gameObject.SetActive(false);

                _mode = value;
            }
        }

        private void OnEnable()
        {
            GameManager.IncBusy();

            popups.gameObject.SetActive(false);

            pieces.transform.DetachAndDestroyChildren();

            foreach(var tile in tileDatabase.prefabs)
                GeneratePreview(tile);

            previewParent.DetachAndDestroyChildren();

            pointerAction.action.performed += OnPointerMoved;
            pointerDownAction.action.performed += OnPointerDown;

            selectionRect.gameObject.SetActive(false);

            drawTool.isOn = true;

            GameManager.Instance.ClearTiles();
        }

        private void OnDisable()
        {
            if(GameManager.Instance != null)
                GameManager.DecBusy();
            pointerAction.action.performed -= OnPointerMoved;
            pointerDownAction.action.performed -= OnPointerDown;
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
            tileObject.GetComponent<Button>().onClick.AddListener(() => {
                selectedTile = prefab;
            });
            tileObject.GetComponent<RawImage>().texture = t;            
        }

        public void OnToolChanged()
        {
            if (drawTool.isOn)
                mode = Mode.Draw;
            else if (selectTool.isOn)
                mode = Mode.Select;
            else if (eraseTool.isOn)
                mode = Mode.Erase;
            else if (wireTool.isOn)
                mode = Mode.Wire;
        }

        private void HandleDrag(DragState state, Vector2Int cell)
        {
            switch (mode)
            {
                case Mode.Draw:
                    if (null == selectedTile)
                        return;

                    if (selectedTile.info.layer == TileLayer.Dynamic && GameManager.Instance.HasCellTiles(cell))
                        foreach (var actor in GameManager.Instance.GetCellTiles(cell))
                            if (!actor.info.allowDynamic)
                                return;

                    GameManager.Instance.ClearTile(cell, selectedTile.info.layer);

                    // Destroy all other instances of this tile regardless of variant
                    if (!selectedTile.info.allowMultiple)
                    {
                        foreach (var actor in GameManager.GetTiles().Where(a => a.info == selectedTile))
                        {
                            Destroy(actor.gameObject);

                            // Replace the static actor with a floor so we dont leave a hole
                            if (selectedTile.info.layer == TileLayer.Static)
                                InstantiateTile(floorTile, actor.cell);
                        }
                    }

                    // Automatically add floor
                    if (selectedTile.info.layer != TileLayer.Static)
                        InstantiateTile(floorTile, cell);

                    InstantiateTile(selectedTile, cell);
                    break;

                case Mode.Erase:
                    GameManager.Instance.ClearTile(cell);
                    break;

                case Mode.Select:
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
                            }
                            break;
                        }

                        case DragState.Update:
                        {
                            if(null == dragWire && dragStart != dragEnd && hasSelection && GetTile(dragStart).info.allowWireOutputs)
                            {
                                dragWire = Instantiate(dragWirePrefab).GetComponent<LineRenderer>();
                                dragWire.positionCount = 2;
                                dragWire.SetPosition(0, GameManager.CellToWorld(dragStart));
                                dragWire.SetPosition(1, GameManager.CellToWorld(cell));
                            }
                            else if(dragWire != null)
                            {
                                dragWire.SetPosition(1, GameManager.CellToWorld(cell));
                            }
                            break;
                        }

                        case DragState.End:
                        {
                            if(dragWire != null)
                            {
                                var wire = GameManager.Instance.InstantiateWire(GetTile(dragStart), GetTile(dragEnd));
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

            //var cell = GameManager.WorldToCell(Camera.main.ScreenToWorldPoint(obj.ReadValue<Vector2>()) + new Vector3(0.5f, 0.5f, 0));
        }

        private Vector2Int pointerCell;

        private void OnPointerMoved(InputAction.CallbackContext ctx)
        {
            var cell = GameManager.WorldToCell(Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>()) + new Vector3(0.5f, 0.5f, 0));
            if (cell != pointerCell)
                pointerCell = cell;

            if(dragging && dragEnd != pointerCell)
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

            if (results.Count <= 0 || results[0].gameObject.GetComponent<UICanvas>() == null)
                return;

            if (mouseDown)
            {
                dragging = true;
                dragStart = pointerCell;
                dragEnd = pointerCell;
                HandleDrag(DragState.Begin, pointerCell);
            }
            else
            {
                dragging = false;
                dragEnd = pointerCell;
                HandleDrag(DragState.End, pointerCell);
            }
        }

        private Tile InstantiateTile (Tile prefab, Vector2Int cell)
        {
            var tile = GameManager.Instance.InstantiateTile(prefab, cell);
            if (null == tile)
                return null;

            tile.gameObject.AddComponent<TileEditorInfo>().prefab = prefab;

            return tile;
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

        public void OnSavePuzzleName()
        {
            if (string.IsNullOrEmpty(puzzleNameInput.text) || String.Compare(puzzleNameInput.text, "unnamed", true) == 0)
                return;

            puzzleName.text = puzzleNameInput.text;
            selectedPuzzle = System.IO.Path.Combine(Application.dataPath, $"Puzzles/{puzzleName.text}.puzzle");

            HidePopup();
            Save();
        }

        public void OnCancelPopup()
        {
            HidePopup();
        }

        public void Save()
        {
            var puzzle = ScriptableObject.CreateInstance<Puzzle>();
            puzzle.Save(GameManager.Instance.transform.GetChild(1));

            System.IO.File.WriteAllText(selectedPuzzle, JsonUtility.ToJson(puzzle));
        }

        public void OnNewButton()
        {
            selectedPuzzle = null;
            puzzleName.text = "Unnamed";
            GameManager.Instance.ClearTiles();
            HidePopup();
        }

        public void OnLoadButton()
        {
            loadPopupFiles.DetachAndDestroyChildren();

            var files = System.IO.Directory.GetFiles(System.IO.Path.Combine(Application.dataPath, "Puzzles"), "*.puzzle");
            foreach(var file in files)
            {
                var fileGameObject = Instantiate(loadPopupFilePrefab, loadPopupFiles);
                fileGameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(file);
                var button = fileGameObject.GetComponent<Button>();
                button.onClick.AddListener(() => {
                    selectedPuzzle = file;
                    button.Select();
                });
            }

            ShowPopup(loadPopup);
        }

        public void OnLoadButtonConfirm()
        {
            Load(selectedPuzzle);
        }

        public void OnStopButton()
        {
            if (!playing)
                return;

            GameManager.ClearBusy();
            GameManager.IncBusy();

            playing = false;
            playButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            Load(selectedPuzzle);
        }

        public void OnPlayButton()
        {
            if (playing)
                return;

            GameManager.ClearBusy();

            playButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);

            playing = true;
            Save();
        }

        public void Load(string file)
        {
            var puzzle = ScriptableObject.CreateInstance<Puzzle>();
            JsonUtility.FromJsonOverwrite(File.ReadAllText(file), puzzle);
            puzzleName.text = Path.GetFileNameWithoutExtension(file);            

            GameManager.Instance.ClearTiles();
            if(puzzle.tempTiles != null)
            {
                var tiles = new List<Tile>();
                foreach (var serializedTile in puzzle.tempTiles)
                {
                    var tile = InstantiateTile(serializedTile.prefab.GetComponent<Tile>(), serializedTile.cell);
                    tiles.Add(tile);
                }

                if(puzzle.tempWires!=null)
                    foreach (var serializedWire in puzzle.tempWires)
                        AddWire(tiles[serializedWire.from], tiles[serializedWire.to]);
            }

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
            return GameManager.Instance.GetCellTiles(cell)?[0];
        }

        private Wire AddWire(Tile input, Tile output)
        {
            return GameManager.Instance.InstantiateWire(input, output);
        }

        private void RemoveWire(Tile input, Tile output)
        {
            for (int i = output.inputs.Count - 1; i >= 0; i--)
                if (output.inputs[i].input == input)
                    output.inputs.RemoveAt(i);

            for (int i = input.outputs.Count - 1; i >= 0; i--)
                if (input.outputs[i].output == output)
                    input.outputs.RemoveAt(i);
        }
    }
}
