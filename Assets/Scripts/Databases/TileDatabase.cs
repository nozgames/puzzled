using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class TileDatabase : AddressableDatabase<GameObject>
    {
        [Header("Preview")]
        [SerializeField] private Camera previewCamera = null;
        [SerializeField] private Transform previewParent = null;

        private static TileDatabase instance = null;

        private Tile[] _tiles = null;

        private Dictionary<Guid, Texture2D> _tilePreviews = new Dictionary<Guid, Texture2D>();

        private void Awake()
        {
            instance = this;
        }

        public static Tile GetTile(Guid guid) => instance.GetAsset(guid)?.GetComponent<Tile>();

        public static Guid GetGuid(Tile tile) => instance.GetGuid(tile.gameObject);

        public static Tile[] GetTiles() => instance._cache.Values.Select(t => t.GetComponent<Tile>()).ToArray();

        protected override string label => "tile";

        /// <summary>
        /// Returns the tile preview for a given tile
        /// </summary>
        public static Texture2D GetPreview(Tile tile) => GetPreview(tile.guid);

        /// <summary>
        /// Returns the tile preview for a given tile guid
        /// </summary>
        public static Texture2D GetPreview (Guid guid)
        {
            if (instance._tilePreviews.TryGetValue(guid, out var preview))
                return preview;

            var tile = GetTile(guid);
            if (null == tile)
                return null;

            preview = instance.GeneratePreview(tile);
            instance._tilePreviews[guid] = preview;
            return preview;
        }

        /// <summary>
        /// Generates a preview for a given tile
        /// </summary>
        private Texture2D GeneratePreview(Tile prefab)
        {
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

            return t;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            _tiles = instance._cache.Select(kv => {
                var tile = kv.Value.GetComponent<Tile>();
                tile.guid = kv.Key;
                return tile;
            }).ToArray();
        }
    }
}
