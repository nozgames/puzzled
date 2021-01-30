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
        [SerializeField] private Puzzle previewPuzzle = null;
        [SerializeField] private Camera previewCamera = null;

        [Header("Ports")]
        [SerializeField] private Sprite _powerPortIcon = null;
        [SerializeField] private Sprite _signalPortIcon = null;
        [SerializeField] private Sprite _numberPortIcon = null;

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

        public static bool isLoaded => _instance != null && _instance.loaded;

        private void Awake()
        {
            _instance = this;
        }
        
        public static void Shutdown()
        {
            _instance.previewPuzzle.Destroy();
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
        public static Texture2D GetPreview(Tile tile) => tile == null ? null : GetPreview(tile.guid);

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
            var puzzle = GameManager.puzzle;

            GameManager.puzzle = previewPuzzle;
            var tile = previewPuzzle.InstantiateTile(prefab, Cell.zero);
            tile.Send(new StartEvent());
            tile.Send(new PreviewStartEvent());

            var renderers = tile.GetComponentsInChildren<Renderer>();
            var max = tile.transform.position;
            var min = tile.transform.position;
            foreach (var renderer in renderers)
            {
                max = Vector3.Max(max, renderer.bounds.max);
                min = Vector3.Min(min, renderer.bounds.min);
            }

            // Position camera to frame the content
            var extents = ((max - min));
            var size = Mathf.Max(extents.x, Mathf.Max(extents.y, extents.z));
            var distance = (size * 0.75f) / Mathf.Abs(Mathf.Sin(previewCamera.fieldOfView * Mathf.Deg2Rad * 0.5f));

            previewCamera.transform.position =
                // Target position
                tile.transform.position + new Vector3(0, (max.y + min.y) * 0.5f, 0)

                // Zoom to frame entire target
                + (distance * -Vector3.Normalize(previewCamera.transform.forward));

            previewCamera.Render();

            var t = new Texture2D(previewCamera.targetTexture.width, previewCamera.targetTexture.height, TextureFormat.ARGB32, false, true);
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;

            RenderTexture.active = previewCamera.targetTexture;
            t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
            t.Apply();

            tile.Destroy();

            GameManager.puzzle = puzzle;

            return t;
        }

        /// <summary>
        /// Return the icon for the given port
        /// </summary>
        /// <param name="port">Port</param>
        /// <returns>Icon as sprite</returns>
        public static Sprite GetPortIcon(Port port)
        {
            if (null == port)
                return null;

            switch (port.type)
            {
                case PortType.Number: return _instance._numberPortIcon;
                case PortType.Signal: return _instance._signalPortIcon;
                case PortType.Power: return _instance._powerPortIcon;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            _tiles = _instance._cache.Select(kv => {
                var tile = kv.Value.GetComponent<Tile>();
                tile.guid = kv.Key;
                return tile;
            })
                .OrderBy(t => t.info.layer)
                .ThenBy(t => t.info.name)
                .ThenBy(t => t.name)
                .ToArray();

            // Build the tile properties array for each tile 
            foreach(var tile in _tiles)
            {
                var properties = _tileProperties[tile.guid] =
                    tile.GetComponentsInChildren<TileComponent>()
                        .SelectMany(tc => tc.GetType().GetFlattenedProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                            (tc, p) => new TileProperty (p, p.GetCustomAttribute<EditableAttribute>(), p.GetCustomAttribute<PortAttribute>()))
                        .Where(ep => ep.editable != null)
                        .ToArray();

                // Ensure there is only one signal/power output
                if (properties.Count(p => p.port != null && p.port.flow == PortFlow.Output && p.port.type != PortType.Number) > 1)
                    throw new InvalidOperationException($"{tile.name}: Multiple signal/power outputs are not allowed");

                // Ensure there is only one number output port
                if (properties.Count(p => p.port != null && p.port.flow == PortFlow.Output && p.port.type == PortType.Number) > 1)
                    throw new InvalidOperationException($"{tile.name}: Multiple number outputs are not allowed");

#if false
                // If there is at least one output then there needs to be a legacy output
                if (properties.Count(p => p.port != null && p.port.flow == PortFlow.Output) > 0 &&
                   properties.Count(p => p.port != null && p.port.flow == PortFlow.Output && p.port.legacy) <= 0)
                    throw new InvalidOperationException($"{tile.name}: At least one legacy output must be specified");

                // If there is at least one input then there must be a legacy input
                if (properties.Count(p => p.port != null && p.port.flow == PortFlow.Input) > 0 &&
                   properties.Count(p => p.port != null && p.port.flow == PortFlow.Input && p.port.legacy) <= 0)
                    throw new InvalidOperationException($"{tile.name}: At least one legacy input must be specified");
#endif
            }
        }
    }
}
