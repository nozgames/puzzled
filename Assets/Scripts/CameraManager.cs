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
        public const int MinZoomLevel = 5;

        /// <summary>
        /// Maximum zoom level
        /// </summary>
        public const int MaxZoomLevel = 20;

        [Header("General")]
        [SerializeField] private Camera _cameraPlayer = null;
        [SerializeField] private Camera _cameraEditor = null;
        [SerializeField] private Camera _cameraEditorLogic = null;

        [Header("Layers")]
        [SerializeField] [Layer] private int floorLayer = 0;
        [SerializeField] [Layer] private int staticLayer = 0;
        [SerializeField] [Layer] private int dynamicLayer = 0;
        [SerializeField] [Layer] private int logicLayer = 0;

        [Header("Background")]
        [SerializeField] private Background _defaultBackground = null;
        [SerializeField] private MeshRenderer _fog = null;
        [SerializeField] private Material _gridMaterial = null;

        /// <summary>
        /// Current background (needed?)
        /// </summary>
        private Material _gridMaterialInstance;

        /// <summary>
        /// Current zoom level
        /// </summary>
        private int _zoomLevel = DefaultZoomLevel;

        /// <summary>
        /// Target position
        /// </summary>
        private Vector3 _targetPosition;

        /// <summary>
        /// Current background
        /// </summary>
        private Background _background = null;

        private static CameraManager _instance = null;

        /// <summary>
        /// True if the camera manager is using the editor camera
        /// </summary>
        public static bool isEditor {
            get => _instance._cameraEditor.gameObject.activeSelf;
            set {
                _instance._cameraEditor.gameObject.SetActive(value);
                _instance._cameraPlayer.gameObject.SetActive(!value);
            }
        }

        /// <summary>
        /// Returns the default background 
        /// </summary>
        public static Background defaultBackground => _instance._defaultBackground;

        /// <summary>
        /// Returns the camera that is currently active
        /// </summary>
        public static Camera activeCamera => _instance._cameraEditor.gameObject.activeSelf ? _instance._cameraEditor : _instance._cameraPlayer;

        /// <summary>
        /// Returns the material used for rendering grids
        /// </summary>
        public static Material gridMaterial => _instance._gridMaterialInstance;

        /// <summary>
        /// Get/Set the camera state
        /// </summary>
        public static CameraState state {
            get => new CameraState {
                position = activeCamera.transform.position,
                orthographicSize = activeCamera.orthographicSize,
                background = _instance._background,
                editor = isEditor
            };
            set {
                // Make sure we set editor first so we alter the correct camera 
                isEditor = value.editor;

                activeCamera.transform.position = value.position;
                if (activeCamera.orthographic)
                    activeCamera.orthographicSize = value.orthographicSize;

                var background = state.background ?? _instance._defaultBackground;
                _instance._fog.material.color = background.color;
                _instance._gridMaterialInstance.color = background.gridColor;
            }
        }

        public void Initialize ()
        {
            _instance = this;

            _gridMaterialInstance = new Material(_instance._gridMaterial);
            _gridMaterialInstance.color = _defaultBackground.gridColor;

            _fog.material.color = Color.white;
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
        public static Vector3 ScreenToWorld(Vector3 screen) => activeCamera.ScreenToWorldPoint(screen);

        private static bool LerpBackgroundColor(Tween tween, float t)
        {
            var lerped = tween.Param1 * (1f - t) + tween.Param2 * t;
            _instance._fog.material.color = new Color(lerped.x, lerped.y, lerped.z, 1.0f);
            return true;
        }

        private bool LerpOrthographicSize(Tween tween, float t)
        {
            activeCamera.orthographicSize = Mathf.Lerp(tween.Param1.x, tween.Param1.y, t) / 2;
            return true;
        }

        /// <summary>
        /// Transition camera from one state to another
        /// </summary>
        /// <param name="position">Target position of the camera</param>
        /// <param name="zoomLevel">Zoom level to transition to</param>
        /// <param name="background">Background</param>
        /// <param name="transitionTime">Time to transition in ticks (0 for instant)</param>
        public static void Transition (Vector3 position, int zoomLevel, Background background, int transitionTime)
        {
            var camera = activeCamera;
            var targetBackground = background ?? _instance._defaultBackground;

            // Clamp zoom
            zoomLevel = Mathf.Clamp(zoomLevel, MinZoomLevel, MaxZoomLevel);

            // Change grid color
            _instance._gridMaterialInstance.color = targetBackground.gridColor;

            // Handle non-animated transition
            if (transitionTime == 0)
            {
                if (camera.orthographic)
                    camera.orthographicSize = zoomLevel;

                // Change background color
                _instance._fog.material.color = targetBackground.color;

                camera.transform.position = Frame(camera, position, zoomLevel);

                _instance._zoomLevel = zoomLevel;
                _instance._background = targetBackground;
                _instance._targetPosition = position;
                return;
            }

            // Busy during a transition
            GameManager.busy++;

            var tweenGroup = Tween.Group();

            // Orthographic zoom
            if(camera.orthographic && camera.orthographicSize != zoomLevel)
                tweenGroup.Child(Tween.Custom(_instance.LerpOrthographicSize, new Vector4(camera.orthographicSize, zoomLevel, 0), Vector4.zero));

            // Move the camera if needed
            var targetPosition = Frame(camera, position, zoomLevel);
            if (targetPosition != camera.transform.position)
                tweenGroup
                    .Child(Tween.Move(camera.transform.position, targetPosition, false).Target(camera.gameObject));

            if (_instance._fog.material.color != targetBackground.color)
                tweenGroup.Child(Tween.Custom(LerpBackgroundColor, _instance._fog.material.color, targetBackground.color));

            tweenGroup
                .Duration(transitionTime * GameManager.tick)
                .EaseInOutCubic()
                .OnStop(_instance.OnTransitionComplete)
                .Start(_instance.gameObject);

            _instance._zoomLevel = zoomLevel;
            _instance._background = targetBackground;
            _instance._targetPosition = position;
        }

        /// <summary>
        /// Transition to a different background
        /// </summary>
        /// <param name="background">New background</param>
        /// <param name="transitionTime">Time to transition</param>
        public static void Transition (Background background, int transitionTime) =>
            Transition(_instance._targetPosition, _instance._zoomLevel, background, transitionTime);

        /// <summary>
        /// Adjust the active camera zoom by the given amount
        /// </summary>
        /// <param name="delta">delta zoom level</param>
        public static void AdjustZoom (int delta) =>
            Transition(_instance._targetPosition, _instance._zoomLevel + delta, _instance._background, 0);

        /// <summary>
        /// Pan the camera by the given amount in world coordinates
        /// </summary>
        /// <param name="pan"></param>
        public static void Pan (Vector3 pan) =>
            Transition(_instance._targetPosition + pan, _instance._zoomLevel, _instance._background, 0);

        /// <summary>
        /// Called when a transition is complete to disable the busy state
        /// </summary>
        private void OnTransitionComplete() => GameManager.busy--;

        public static void Play()
        {
        }

        public static void Stop()
        {
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
            var camera = activeCamera;
            if (layer == TileLayer.Logic && isEditor)
                camera = _instance._cameraEditorLogic;

            if (show)
                camera.cullingMask |= (1 << TileLayerToObjectLayer(layer));
            else
                camera.cullingMask &= ~(1 << TileLayerToObjectLayer(layer));
        }

        /// <summary>
        /// Frame the camera on the given position using the given zoom level 
        /// </summary>
        /// <param name="camera">Camera to frame</param>
        /// <param name="position">Position to focus on</param>
        /// <param name="zoom">Zoom level in number of vertical tiles that should be visible</param>
        /// <returns></returns>
        private static Vector3 Frame (Camera camera, Vector3 position, float zoom)
        {
            var foreshorten = 0; // camera.orthographic ? 0.0f : (0.5f * ((0.5f) / Mathf.Abs(Mathf.Sin(camera.fieldOfView * Mathf.Deg2Rad * 0.5f))));
            var distance = ((zoom - 1) * 0.5f) / Mathf.Abs(Mathf.Sin(camera.fieldOfView * Mathf.Deg2Rad * 0.5f));
            return 
                // Target position
                position

                // Zoom to frame entire target
                + (distance * -Vector3.Normalize(camera.transform.forward))
                
                // Adjust for foreshortening 
                - (foreshorten * Vector3.Dot(-Vector3.Normalize(camera.transform.forward), Vector3.forward) * Vector3.forward);
        }
    }
}
