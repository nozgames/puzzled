using System;
using System.Reflection;
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

        /// <summary>
        /// Singleton instance of the tile database
        /// </summary>
        private static TileDatabase _instance = null;

        /// <summary>
        /// Cached array for returning empty lists of properties when a guid is not found
        /// </summary>
        private TileProperty[] _emptyProperties = new TileProperty[0];

        /// <summary>
        /// Array of all known tile prefabs
        /// </summary>
        private Tile[] _tiles = null;

        /// <summary>
        /// Dictionary of tile guid to tile previews
        /// </summary>
        private Dictionary<Guid, Texture2D> _tilePreviews = new Dictionary<Guid, Texture2D>();

        /// <summary>
        /// Dictionary of tile guid to tile properties
        /// </summary>
        private Dictionary<Guid, TileProperty[]> _tileProperties = new Dictionary<Guid, TileProperty[]>();

        private void Awake()
        {
            _instance = this;
        }

        /// <summary>
        /// Return the tile prefab for the given guid
        /// </summary>
        /// <param name="guid">Tile prefab guid</param>
        /// <returns>Tile prefab that maches the given guid or null</returns>
        public static Tile GetTile(Guid guid) => _instance.GetAsset(guid)?.GetComponent<Tile>();

        /// <summary>
        /// Return the guid for the guid for the given tile prefab
        /// </summary>
        /// <param name="tile">Tile prefab</param>
        /// <returns>Guid of the tile prefab or Guid.Empty if not found</returns>
        public static Guid GetGuid(Tile tile) => _instance.GetGuid(tile.gameObject);

        /// <summary>
        /// Return the array of all known tile prefabs
        /// </summary>
        public static Tile[] GetTiles() => _instance._tiles;

        /// <summary>
        /// Lable used to identify tiles in the addressables database
        /// </summary>
        protected override string label => "tile";

        /// <summary>
        /// Return the tile properties for the given tile
        /// </summary>
        /// <param name="tile">Input tile</param>
        /// <returns>Array of tile properties for the given tile</returns>
        public static TileProperty[] GetProperties(Tile tile) => 
            _instance._tileProperties.TryGetValue(tile.guid, out var props) ? props : _instance._emptyProperties;

        /// <summary>
        /// Returns the tile preview for a given tile
        /// </summary>
        public static Texture2D GetPreview(Tile tile) => GetPreview(tile.guid);

        /// <summary>
        /// Returns the tile preview for a given tile guid
        /// </summary>
        public static Texture2D GetPreview (Guid guid)
        {
            if (_instance._tilePreviews.TryGetValue(guid, out var preview))
                return preview;

            var tile = GetTile(guid);
            if (null == tile)
                return null;

            preview = _instance.GeneratePreview(tile);
            _instance._tilePreviews[guid] = preview;
            return preview;
        }

        /// <summary>
        /// Generates a preview for a given tile
        /// </summary>
        private Texture2D GeneratePreview(Tile prefab)
        {
            previewParent.gameObject.SetActive(true);

            var tile = Instantiate(prefab.gameObject, previewParent).GetComponent<Tile>();
            tile.gameObject.SetChildLayers(LayerMask.NameToLayer("Preview"));

            previewCamera.Render();

            var t = new Texture2D(previewCamera.targetTexture.width, previewCamera.targetTexture.height, TextureFormat.ARGB32, false);
            t.filterMode = FilterMode.Point;

            RenderTexture.active = previewCamera.targetTexture;
            t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
            t.Apply();

            tile.Destroy();

            previewParent.gameObject.SetActive(false);

            return t;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            _tiles = _instance._cache.Select(kv => {
                var tile = kv.Value.GetComponent<Tile>();
                tile.guid = kv.Key;
                return tile;
            })
                .OrderBy(t => t.name)
                .ToArray();

            // Build the tile properties array for each tile 
            foreach(var tile in _tiles)
            {
                _tileProperties[tile.guid] =
                    tile.GetComponentsInChildren<TileComponent>()
                        .SelectMany(tc =>
                            tc.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance),
                            (tc, p) => new TileProperty { property = p, editable = p.GetCustomAttribute<EditableAttribute>() })
                        .Where(ep => ep.editable != null)
                        .ToArray();
            }
        }
    }
}
