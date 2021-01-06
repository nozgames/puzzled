using NoZ;
using UnityEngine;

namespace Puzzled
{
    /// <summary>
    /// Current state of the camera
    /// </summary>
    public struct CameraState
    {
        public Vector3 position;
        public float orthographicSize;
        public Background background;
        public bool editor;
    }

    /// <summary>
    /// Manages the camera
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        /// <summary>
        /// Default zoom level
        /// </summary>
        public const int DefaultZoomLevel = 12;

        /// <summary>
        /// Minimum zoom level
        /// </summary>
        public const int MinZoomLevel = 2;

        /// <summary>
        /// Maximum zoom level
        /// </summary>
        public const int MaxZoomLevel = 20;

        [Header("General")]
        [SerializeField] private Camera _cameraPlayer = null;
        [SerializeField] private Camera _cameraEditor = null;

        [Header("Layers")]
        [SerializeField] [Layer] private int floorLayer = 0;
        [SerializeField] [Layer] private int staticLayer = 0;
        [SerializeField] [Layer] private int dynamicLayer = 0;
        [SerializeField] [Layer] private int logicLayer = 0;
        [SerializeField] private LayerMask playLayers = 0;
        [SerializeField] private LayerMask defaultLayers = 0;

        [Header("Background")]
        [SerializeField] private Material _floorGradientMaterial = null;
        [SerializeField] private Material _gridMaterial = null;

        private Cell _cell = Cell.invalid;
        private int _zoomLevel;
        private Background _background;
        private Material _floorGradientMaterialInstance;
        private Material _gridMaterialInstance;
        private float _z;
        private Camera _cameraCurrent;

        private static CameraManager _instance = null;

        /// <summary>
        /// Cell the camera is centered on
        /// </summary>
        public static Cell cell => _instance._cell;

        public static Material floorGradientMaterial => _instance._floorGradientMaterialInstance;

        public static Material gridMaterial => _instance._gridMaterialInstance;

        public static bool isEditor {
            get => _instance._cameraEditor.gameObject.activeSelf;
            set {
                _instance._cameraEditor.gameObject.SetActive(value);
                _instance._cameraPlayer.gameObject.SetActive(!value);
            }
        }

        /// <summary>
        /// Current background
        /// </summary>
        public static Background background => _instance._background == null ? _instance._defaultBackground : _instance._background;

        /// <summary>
        /// Get/Set the camera state
        /// </summary>
        public static CameraState state {
            get => new CameraState {
                position = _instance._cameraCurrent.transform.position,
                orthographicSize = _instance._cameraCurrent.orthographicSize,
                background = background,
                editor = isEditor
            };
            set {
                _instance._cameraCurrent.transform.position = value.position;
                _instance._cameraCurrent.orthographicSize = value.orthographicSize;
                TransitionToBackground(value.background, 0);
                isEditor = value.editor;
            }
        }

        public void Initialize ()
        {
            _cameraCurrent = _cameraEditor;
            _z = _cameraCurrent.transform.position.z;

            _zoomLevel = (int)_cameraCurrent.orthographicSize;

            // Set default camera mask layers
            _cameraCurrent.cullingMask = defaultLayers;

            _instance._cameraCurrent.backgroundColor = _instance._defaultBackground.color;

            _instance._floorGradientMaterialInstance = new Material(_instance._floorGradientMaterial);
            _instance._floorGradientMaterialInstance.color = _instance._cameraCurrent.backgroundColor;

            _instance._gridMaterialInstance = new Material(_instance._gridMaterial);
            _instance._gridMaterialInstance.color = _defaultBackground.gridColor;
        }

        private void OnEnable()
        {
            _instance = this;
        }

        private void OnDisable()
        {
            _instance = null;
        }

        [SerializeField] private Background _defaultBackground = null;

        /// <summary>
        /// Convert the given screen coordinate to a world coordinate
        /// </summary>
        /// <param name="screen">Screen coordinate</param>
        /// <returns>World coordinate</returns>
        public static Vector3 ScreenToWorld(Vector3 screen) => _instance._cameraCurrent.ScreenToWorldPoint(screen);

        public static void JumpToCell(Cell cell, int zoomLevel=-1)
        {
            _instance._cameraCurrent.transform.position = Puzzle.current.grid.CellToWorld(cell) + Vector3.forward * _instance._z;

            if (zoomLevel >= 0)
            {
                _instance._cameraCurrent.orthographicSize = zoomLevel / 2;
                _instance._zoomLevel = zoomLevel;
            }

            _instance._cell = cell;
        }

        public static void TransitionToBackground(Background to, int transitionTime = 4)
        {
            if (_instance._background == to)
                return;

            to = to == null ? _instance._defaultBackground : to;

            _instance._gridMaterialInstance.color = to.gridColor;

            var from = background;
            _instance._background = to;

            if (transitionTime == 0)
            {
                _instance._floorGradientMaterialInstance.color = to.color;
                _instance._cameraCurrent.backgroundColor = to.color;
            } 
            else
            {
                Tween.Custom(LerpBackgroundColor, from.color, to.color)
                    .Duration(transitionTime * GameManager.tick)
                    .Start(_instance);
            }
        }

        private static bool LerpBackgroundColor(Tween tween, float t)
        {
            var lerped = tween.Param1 * (1f - t) + tween.Param2 * t;
            var color = new Color(lerped.x, lerped.y, lerped.z, 1.0f);
            _instance._floorGradientMaterialInstance.color = color;
            _instance._cameraCurrent.backgroundColor = color;
            return true;
        }

        public static void TransitionToCell(Cell cell, int zoomLevel, int transitionTime)
        {
            if ((cell == _instance._cell) && (zoomLevel == _instance._zoomLevel))
                return; // nothing to do

            GameManager.busy++;

            var tweenGroup = Tween.Group();

            if (zoomLevel != _instance._zoomLevel)
                tweenGroup.Child(Tween.Custom(_instance.CameraZoomUpdate, new Vector4(_instance._zoomLevel, zoomLevel, 0), Vector4.zero));

            if (cell != _instance._cell)
                tweenGroup.Child(Tween.Move(Puzzle.current.grid.CellToWorld(_instance._cell) + Vector3.forward * _instance._z, Puzzle.current.grid.CellToWorld(cell) + Vector3.forward * _instance._z, false));

            float duration = transitionTime * GameManager.tick;

            tweenGroup.Duration(duration)
                .OnStop(_instance.OnCameraTransitionComplete)
                .Start(_instance._cameraCurrent.gameObject);

            _instance._cell = cell;
            _instance._zoomLevel = zoomLevel;
        }

        /// <summary>
        /// Pan the camera by the given amount in world coordinates
        /// </summary>
        /// <param name="pan"></param>
        public static void Pan (Vector3 pan)
        {
            _instance._cameraCurrent.transform.position += Vector3.Scale(pan, new Vector3(1, 1, 0));
            _instance._cell = Puzzle.current.grid.WorldToCell(_instance._cameraCurrent.transform.position);
        }

        private bool CameraZoomUpdate(Tween tween, float t)
        {
            _cameraCurrent.orthographicSize = Mathf.Lerp(tween.Param1.x, tween.Param1.y, t) / 2;

            return true;
        }

        private void OnCameraTransitionComplete()
        {
            GameManager.busy--;
        }

        public static void Play()
        {
            _instance._cameraCurrent.cullingMask = _instance.playLayers;

            if (_instance._cell == Cell.invalid)
            {
                if (Puzzle.current.playerCell != Cell.invalid)
                    _instance._cell = Puzzle.current.playerCell;
                else
                    _instance._cell = Puzzle.current.grid.WorldToCell(_instance._cameraCurrent.transform.position);
            }
        }

        public static void Stop()
        {
            _instance._cameraCurrent.cullingMask = _instance.defaultLayers;
        }

        /// <summary>
        /// Return the object layer that matches the given tile layer
        /// </summary>
        /// <param name="tileLayer">Tile layer</param>
        /// <returns>Object layer</returns>
        public static int TileLayerToObjectLayer(TileLayer tileLayer)
        {
            switch (tileLayer)
            {
                case TileLayer.Floor:
                    return _instance.floorLayer;

                case TileLayer.Static:
                    return _instance.staticLayer;

                case TileLayer.Dynamic:
                    return _instance.dynamicLayer;

                case TileLayer.Logic:
                    return _instance.logicLayer;
            }

            return 0;
        }

        /// <summary>
        /// Show/Hide a tile layer
        /// </summary>
        /// <param name="layer">Tile layer to show/hide</param>
        /// <param name="show">True to show the layer, false to hide it</param>
        public static void ShowLayer(TileLayer layer, bool show=true)
        {
            if (show)
                _instance._cameraCurrent.cullingMask |= (1 << TileLayerToObjectLayer(layer));
            else
                _instance._cameraCurrent.cullingMask &= ~(1 << TileLayerToObjectLayer(layer));
        }
    }
}
