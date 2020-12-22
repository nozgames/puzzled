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
        [SerializeField] private Camera _camera = null;

        [Header("Layers")]
        [SerializeField] [Layer] private int floorLayer = 0;
        [SerializeField] [Layer] private int staticLayer = 0;
        [SerializeField] [Layer] private int dynamicLayer = 0;
        [SerializeField] [Layer] private int logicLayer = 0;
        [SerializeField] private LayerMask playLayers = 0;
        [SerializeField] private LayerMask defaultLayers = 0;

        private Cell _cell = Cell.invalid;
        private int _zoomLevel;

        private static CameraManager _instance = null;

        /// <summary>
        /// Cell the camera is centered on
        /// </summary>
        public static Cell cell => _instance._cell;

        /// <summary>
        /// Get/Set the camera state
        /// </summary>
        public static CameraState state {
            get => new CameraState {
                position = _instance._camera.transform.position,
                orthographicSize = _instance._camera.orthographicSize
            };
            set {
                _instance._camera.transform.position = value.position;
                _instance._camera.orthographicSize = value.orthographicSize;
            }
        }

        public void Initialize ()
        {
            _zoomLevel = (int)_camera.orthographicSize;

            // Set default camera mask layers
            _camera.cullingMask = defaultLayers;
        }

        private void OnEnable()
        {
            _instance = this;
        }

        private void OnDisable()
        {
            _instance = null;
        }

        /// <summary>
        /// Convert the given screen coordinate to a world coordinate
        /// </summary>
        /// <param name="screen">Screen coordinate</param>
        /// <returns>World coordinate</returns>
        public static Vector3 ScreenToWorld(Vector3 screen) => _instance._camera.ScreenToWorldPoint(screen);

        public static void JumpToCell(Cell cell, int zoomLevel=-1)
        {
            _instance._camera.transform.position = Puzzle.current.grid.CellToWorld(cell);

            if (zoomLevel >= 0)
            {
                _instance._camera.orthographicSize = zoomLevel / 2;
                _instance._zoomLevel = zoomLevel;
            }

            _instance._cell = cell;
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
                tweenGroup.Child(Tween.Move(Puzzle.current.grid.CellToWorld(_instance._cell), Puzzle.current.grid.CellToWorld(cell), false));

            float duration = transitionTime * GameManager.tick;

            tweenGroup.Duration(duration)
                .OnStop(_instance.OnCameraTransitionComplete)
                .Start(_instance._camera.gameObject);

            _instance._cell = cell;
            _instance._zoomLevel = zoomLevel;
        }

        /// <summary>
        /// Pan the camera by the given amount in world coordinates
        /// </summary>
        /// <param name="pan"></param>
        public static void Pan (Vector3 pan)
        {
            _instance._camera.transform.position += Vector3.Scale(pan, new Vector3(1, 1, 0));
            _instance._cell = Puzzle.current.grid.WorldToCell(_instance._camera.transform.position);
        }

        private bool CameraZoomUpdate(Tween tween, float t)
        {
            _camera.orthographicSize = Mathf.Lerp(tween.Param1.x, tween.Param1.y, t) / 2;

            return true;
        }

        private void OnCameraTransitionComplete()
        {
            GameManager.busy--;
        }

        public static void Play()
        {
            _instance._camera.cullingMask = _instance.playLayers;

            if (_instance._cell == Cell.invalid)
            {
                if (Puzzle.current.playerCell != Cell.invalid)
                    _instance._cell = Puzzle.current.playerCell;
                else
                    _instance._cell = Puzzle.current.grid.WorldToCell(_instance._camera.transform.position);
            }
        }

        public static void Stop()
        {
            _instance._camera.cullingMask = _instance.defaultLayers;
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
                _instance._camera.cullingMask |= (1 << TileLayerToObjectLayer(layer));
            else
                _instance._camera.cullingMask &= ~(1 << TileLayerToObjectLayer(layer));
        }
    }
}
