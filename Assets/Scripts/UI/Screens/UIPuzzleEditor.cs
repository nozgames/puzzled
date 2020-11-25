using System;
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

        [SerializeField] private Theme theme = null;
        [SerializeField] private Camera previewCamera = null;
        [SerializeField] private Transform previewParent = null;
        [SerializeField] private GameObject piecePrefab = null;
        [SerializeField] private Transform pieces = null;
        [SerializeField] private RectTransform selectionRect = null;

        [Header("Input")]
        [SerializeField] private InputActionReference pointerAction;
        [SerializeField] private InputActionReference pointerDownAction;

        [Header("Blocks")]
        [SerializeField]
        private BlockGroup[] blockGroups;

        private Mode _mode = Mode.Draw;

        private GameObject selectedTile = null;

        private Mode mode {
            get => _mode;
            set => _mode = value;
        }

        private void OnEnable()
        {
            pieces.transform.DetachAndDestroyChildren();

            foreach(var id in (TileType[])Enum.GetValues(typeof(TileType)))
                GeneratePreview(id);

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

        private void GeneratePreview(TileType tileId)
        {
            var prefab = theme.GetPrefab(tileId);
            if (null == prefab)
                return;

            if (previewParent.childCount > 0)
                previewParent.GetChild(0).gameObject.SetActive(false);

            previewParent.DetachAndDestroyChildren();
            
            var blockObject = Instantiate(prefab, previewParent);
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

                    GameManager.Instance.ClearTile(cell);

                    // Automatically add floor
                    if (selectedTile.GetComponent<PuzzledActor>().tileType != TileType.Floor)
                        GameManager.Instance.InstantiateTile(theme.GetPrefab(TileType.Floor), cell);

                    GameManager.Instance.InstantiateTile(selectedTile, cell);
                    break;

                case Mode.Erase:
                    GameManager.Instance.ClearTile(cell);
                    break;

                case Mode.Select:
                    SetSelectionRect(cell, cell);
                    break;
            }
        }

        private void SetSelectionRect(Vector2Int min, Vector2Int max)
        {
            selectionRect.anchoredPosition = 
                ((Vector2)Camera.main.WorldToScreenPoint(GameManager.CellToWorld(min) - new Vector3(0.5f, 0.5f, 0))) - new Vector2(4,4);

            selectionRect.gameObject.SetActive(true);

            //var cell = GameManager.WorldToCell(Camera.main.ScreenToWorldPoint(obj.ReadValue<Vector2>()) + new Vector3(0.5f, 0.5f, 0));
        }

        private Vector2Int lastCell;

        private void OnPointerMoved(InputAction.CallbackContext ctx)
        {
            var cell = GameManager.WorldToCell(Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>()) + new Vector3(0.5f, 0.5f, 0));
            if (cell != lastCell)
                lastCell = cell;
        }

        public void OnPointerDown(InputAction.CallbackContext ctx)
        {
            var results = new List<UnityEngine.EventSystems.RaycastResult>();

            UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = pointerAction.action.ReadValue<Vector2>() }, results);

            if (ctx.ReadValueAsButton())
                if (results.Count > 0 && results[0].gameObject.GetComponent<UICanvas>() != null)

                {
                    OnCanvasPointerDown(lastCell);
                    //pointerDown.Invoke(lastCell);
                }
        }
    }
}
