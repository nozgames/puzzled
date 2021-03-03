using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Puzzled
{
    public class DatabaseManager : MonoBehaviour
    {
        [Serializable]
        private struct TileMigration
        {
            public Tile from;
            public Tile to;
        }

        [SerializeField] private TileDatabase _tileDatabase = null;
        [SerializeField] private BackgroundDatabase _backgroundDatabase = null;
        [SerializeField] private SoundDatabase _soundDatabase = null;
        [SerializeField] private DecalDatabase _decalDatabase = null;

        [Header("Tiles")]
        [SerializeField] private Puzzle _tilePreviewPuzzle = null;
        [SerializeField] private Camera _tilePreviewCamera = null;
        [SerializeField] private TileMigration[] _tileMigrations = null;

        [Header("Ports")]
        [SerializeField] private Sprite _powerPortIcon = null;
        [SerializeField] private Sprite _signalPortIcon = null;
        [SerializeField] private Sprite _numberPortIcon = null;

        private static DatabaseManager _instance = null;

        private Background[] _backgrounds;
        private Sound[] _sounds;
        private Decal[] _decals;
        private TileProperty[] _emptyProperties = new TileProperty[0];
        private Tile[] _tiles = null;
        private Dictionary<Guid, Tile> _tilesByGuid = new Dictionary<Guid, Tile>();
        private Dictionary<Guid, Texture2D> _tilePreviews = new Dictionary<Guid, Texture2D>();
        private Dictionary<Guid, TileProperty[]> _tileProperties = new Dictionary<Guid, TileProperty[]>();

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        public static void Initialize ()
        {
            if (_instance == null)
                return;

            // Cache list of all background and update the guid of all backgrounds
            _instance._backgrounds = _instance._backgroundDatabase.GetResources()
                .Select(r => r.asset)
                .OrderBy(b => b.name)
                .ToArray();

            // Cache list of all sounds
            _instance._sounds = _instance._soundDatabase.GetResources()
                .Select(r => new Sound { clip = r.asset, guid = r.guid })
                .OrderBy(s => s.clip.name)
                .ToArray();

            // Cache list of all decals
            _instance._decals = _instance._decalDatabase.GetResources()
                .Select(r => new Decal(r.guid, r.asset))
                .OrderBy(d => d.name)
                .ToArray();

            // Cache list of tiles and stuff the guid into it
            _instance._tiles = _instance._tileDatabase.GetResources().Select(r => {
                var tile = r.asset.GetComponent<Tile>();
                if (null == tile)
                    return null;

                tile.guid = r.guid;
                _instance._tilesByGuid[tile.guid] = tile;
                return tile;
            }).Where(t => t != null)
                .OrderBy(t => t.layer)
                .ThenBy(t => t.info.name)
                .ThenBy(t => t.name)
                .ToArray();

            // Handle migrations
            if (_instance._tileMigrations != null)
                foreach (var migration in _instance._tileMigrations)
                    _instance._tilesByGuid[migration.from.guid] = migration.to;

            // Build the tile properties array for each tile 
            foreach (var tile in _instance._tiles)
            {
                var properties = _instance._tileProperties[tile.guid] =
                    tile.GetComponentsInChildren<TileComponent>()
                        .SelectMany(tc => tc.GetType().GetFlattenedProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                            (tc, p) => new TileProperty(p, p.GetCustomAttribute<EditableAttribute>(), p.GetCustomAttribute<PortAttribute>()))
                        .Where(ep => ep.editable != null)
                        .ToArray();

                // Ensure there is only one signal/power output
                if (properties.Count(p => p.port != null && p.port.flow == PortFlow.Output && p.port.type != PortType.Number) > 1)
                    throw new InvalidOperationException($"{tile.name}: Multiple signal/power outputs are not allowed");

                // Ensure there is only one number output port
                if (properties.Count(p => p.port != null && p.port.flow == PortFlow.Output && p.port.type == PortType.Number) > 1)
                    throw new InvalidOperationException($"{tile.name}: Multiple number outputs are not allowed");
            }
        }

        public static void Shutdown()
        {
            if (_instance == null)
                return;

            _instance._tilePreviewPuzzle.Destroy();
            _instance = null;
        }

        /// <summary>
        /// Get array of all available backgrounds
        /// </summary>
        /// <returns>All backgrounds</returns>
        public static Background[] GetBackgrounds() => _instance._backgrounds;

        public static Background GetBackground(Guid guid) => _instance._backgroundDatabase.GetResource(guid).asset;

        public static Sound[] GetSounds() => _instance._sounds;

        public static Sound GetSound(Guid guid)
        {
            var resource = _instance._soundDatabase.GetResource(guid);
            return new Sound { clip = resource.asset, guid = resource.guid };
        }

        public static Decal[] GetDecals() => _instance._decals;

        public static Decal GetDecal (Guid guid)
        {
            var resource = _instance._decalDatabase.GetResource(guid);
            return new Decal(resource.guid, resource.asset);
        }

        /// <summary>
        /// Return the tile prefab for the given guid
        /// </summary>
        /// <param name="guid">Tile prefab guid</param>
        /// <returns>Tile prefab that maches the given guid or null</returns>
        public static Tile GetTile(Guid guid) => _instance._tilesByGuid.TryGetValue(guid, out var tile) ? tile : null;

        /// <summary>
        /// Return the guid for the guid for the given tile prefab
        /// </summary>
        /// <param name="tile">Tile prefab</param>
        /// <returns>Guid of the tile prefab or Guid.Empty if not found</returns>
        public static Guid GetGuid(Tile tile) => _instance._tileDatabase.GetGuid(tile.gameObject);

        /// <summary>
        /// Return the array of all known tile prefabs
        /// </summary>
        public static Tile[] GetTiles() => _instance._tiles;

        /// <summary>
        /// Return the tile properties for the given tile
        /// </summary>
        /// <param name="tile">Input tile</param>
        /// <returns>Array of tile properties for the given tile</returns>
        public static TileProperty[] GetProperties(Tile tile) =>
            _instance._tileProperties.TryGetValue(tile.guid, out var props) ? props : _instance._emptyProperties;


        /// <summary>
        /// Generates a preview for a given tile
        /// </summary>
        private Texture2D GeneratePreview(Tile prefab)
        {
            var puzzle = GameManager.puzzle;

            GameManager.puzzle = _tilePreviewPuzzle;

            var tile = _tilePreviewPuzzle.InstantiateTile(prefab, _tilePreviewPuzzle.grid.LayerToCell(prefab.layer));
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
            var rotation = new Vector3(tile.layer == TileLayer.Logic ? 90 : _tilePreviewCamera.transform.localEulerAngles.x, 0, 0);
            _tilePreviewCamera.transform.localEulerAngles = rotation;
            _tilePreviewCamera.transform.position = CameraManager.Frame(
                tile.transform.position + new Vector3(0, (max.y + min.y) * 0.5f, 0),
                rotation.x,
                size * 1.5f,
                _tilePreviewCamera.fieldOfView
                );

            _tilePreviewCamera.Render();

            var t = new Texture2D(_tilePreviewCamera.targetTexture.width, _tilePreviewCamera.targetTexture.height, TextureFormat.ARGB32, false, true);
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;

            RenderTexture.active = _tilePreviewCamera.targetTexture;
            t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
            t.Apply();

            tile.Destroy();

            GameManager.puzzle = puzzle;

            return t;
        }

        /// <summary>
        /// Returns the tile preview for a given tile
        /// </summary>
        public static Texture2D GetPreview(Tile tile) => tile == null ? null : GetPreview(tile.guid);

        /// <summary>
        /// Returns the tile preview for a given tile guid
        /// </summary>
        public static Texture2D GetPreview(Guid guid)
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
                case PortType.Number:
                    return _instance._numberPortIcon;
                case PortType.Signal:
                    return _instance._signalPortIcon;
                case PortType.Power:
                    return _instance._powerPortIcon;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
