using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIPuzzleEditor : UIScreen
    {
        private enum Mode
        {
            Draw,
            Move
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

            foreach(var id in (TileId[])Enum.GetValues(typeof(TileId)))
                GeneratePreview(id);

            previewParent.DetachAndDestroyChildren();

            GameManager.Instance.ClearTiles();
        }

        private void GeneratePreview(TileId tileId)
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

        private void OnMoveToolButton()
        {
            mode = Mode.Move;
        }

        private void OnDrawToolButton()
        {
            mode = Mode.Draw;
        }

        public void OnCanvasPointerDown(Vector2Int cell)
        {
            if (null == selectedTile)
                return;

            GameManager.Instance.ClearTile(cell);

            // Automatically add floor
            if (selectedTile.GetComponent<PuzzledActor>().id != TileId.Floor)
                GameManager.Instance.InstantiateTile(theme.GetPrefab(TileId.Floor), cell);

            GameManager.Instance.InstantiateTile(selectedTile, cell);
        }
    }
}
