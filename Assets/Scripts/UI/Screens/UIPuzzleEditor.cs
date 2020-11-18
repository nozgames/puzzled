using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIPuzzleEditor : UIScreen
    {
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

        [SerializeField] private Camera previewCamera = null;
        [SerializeField] private Transform previewParent = null;
        [SerializeField] private GameObject piecePrefab = null;
        [SerializeField] private Transform pieces = null;

        [Header("Blocks")]
        [SerializeField]
        private BlockGroup[] blockGroups;

        private void OnEnable()
        {
            pieces.transform.DetachAndDestroyChildren();

            foreach (var group in blockGroups)
                foreach (var block in group.blocks)
                    GeneratePreview(block);

            previewParent.DetachAndDestroyChildren();
        }

        private void GeneratePreview(Block block)
        {
            if (previewParent.childCount > 0)
                previewParent.GetChild(0).gameObject.SetActive(false);

            previewParent.DetachAndDestroyChildren();

            var blockObject = Instantiate(block.prefab, previewParent);
            blockObject.SetChildLayers(LayerMask.NameToLayer("Preview"));

            previewCamera.Render();

            var t = new Texture2D(previewCamera.targetTexture.width, previewCamera.targetTexture.height, TextureFormat.ARGB32, false);
            t.filterMode = FilterMode.Point;

            RenderTexture.active = previewCamera.targetTexture;
            t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
            t.Apply();

            block.preview = t;

            Instantiate(piecePrefab, pieces).GetComponent<RawImage>().texture = t;            
        }

        private void OnMoveToolButton()
        {

        }

        private void OnDrawToolButton()
        {

        }
    }
}
