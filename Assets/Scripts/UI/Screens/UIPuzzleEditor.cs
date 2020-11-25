using System;
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
            Erase
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

        [Header("Input")]
        [SerializeField] private InputActionReference pointerAction;
        [SerializeField] private InputActionReference pointerDownAction;

        [Header("Tiles")]
        [SerializeField] private Tile floorTile = null;
        [SerializeField] private BlockGroup[] blockGroups;

        private Mode _mode = Mode.Draw;

        private Tile selectedTile = null;
        private Vector2Int dragStart;
        private Vector2Int dragEnd;
        private bool dragging;

        private Mode mode {
            get => _mode;
            set => _mode = value;
        }

        private void OnEnable()
        {
            pieces.transform.DetachAndDestroyChildren();

            foreach(var tile in tileDatabase.prefabs)
                GeneratePreview(tile);

            previewParent.DetachAndDestroyChildren();

            pointerAction.action.performed += OnPointerMoved;
            pointerDownAction.action.performed += OnPointerDown;

            selectionRect.gameObject.SetActive(false);

            GameManager.Instance.ClearTiles();
        }

        private void OnDisable()
        {
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

        public void OnSelectToolButton()
        {
            mode = Mode.Select;
        }

        public void OnDrawToolButton()
        {
            selectionRect.gameObject.SetActive(false);
            mode = Mode.Draw;
        }

        public void OnEraseToolButton()
        {
            selectionRect.gameObject.SetActive(false);
            mode = Mode.Erase;
        }

        public void OnCanvasPointerDown(Vector2Int cell)
        {
            switch (mode)
            {
                case Mode.Draw:
                    if (null == selectedTile)
                        return;

                    if (selectedTile.info.layer == TileLayer.Dynamic)
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

        private Vector2Int lastCell;

        private void OnPointerMoved(InputAction.CallbackContext ctx)
        {
            var cell = GameManager.WorldToCell(Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>()) + new Vector3(0.5f, 0.5f, 0));
            if (cell != lastCell)
                lastCell = cell;

            if(dragging && dragEnd != lastCell)
            {
                dragEnd = lastCell;
                UpdateDrag();
            }
        }

        private void UpdateDrag()
        {
            OnCanvasPointerDown(dragEnd);
        }

        private void OnPointerDown(InputAction.CallbackContext ctx)
        {
            var results = new List<UnityEngine.EventSystems.RaycastResult>();

            UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = pointerAction.action.ReadValue<Vector2>() }, results);
            if (results.Count <= 0 || results[0].gameObject.GetComponent<UICanvas>() == null)
                return;

            if (ctx.ReadValueAsButton())
            {
                dragging = true;
                dragStart = lastCell;
                dragEnd = lastCell;
            }
            else
            {
                dragging = false;
            }

            OnCanvasPointerDown(lastCell);
        }

        private Tile InstantiateTile (Tile prefab, Vector2Int cell, int variantIndex=0)
        {
            var tile = GameManager.Instance.InstantiateTile(prefab, cell, variantIndex);
            if (null == tile)
                return null;

            tile.gameObject.AddComponent<TileEditorInfo>().prefab = prefab;

            return tile;
        }

        public void OnSaveButton()
        {
            var puzzle = new Puzzle();
            puzzle.Save(GameManager.Instance.transform.GetChild(1));

            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "Puzzles/test.puzzle"), JsonUtility.ToJson(puzzle));
        }

        public void OnLoadButton()
        {
            var puzzle = ScriptableObject.CreateInstance<Puzzle>();
            JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "Puzzles/test.puzzle")), puzzle);

            GameManager.Instance.ClearTiles();
            if(puzzle.tempTiles != null)
            {
                foreach (var tile in puzzle.tempTiles)
                    InstantiateTile(tile.prefab.GetComponent<Tile>(), tile.cell);
            }
        }
    }
}
