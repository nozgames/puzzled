using NoZ;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    /// <summary>
    /// Current state of the camera
    /// </summary>
    public struct CameraState
    {
        public bool valid;
        public Vector3 target;
        public int pitch;
        public int zoom;
        public bool editor;
        public Background background;
        public Transform followTarget;
        public int cullingMask;
        public bool showLogicTiles;
        public bool showWires;
        public bool showFog;
        public bool showLetterbox;
    }

    public class SharedCameraData
    {
        public List<GameCamera> activeCameras = new List<GameCamera>(16);

        public void AddCamera(GameCamera cam)
        {
            int i = 0;
            for (; i < activeCameras.Count; ++i)
            {
                if (activeCameras[i].priority > cam.priority)
                    break;
            }

            activeCameras.Insert(i, cam);
        }

        public void RemoveCamera(GameCamera cam)
        {
            activeCameras.Remove(cam);
        }
    }

    /// <summary>
    /// Manages the camera
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        /// <summary>
        /// Time to smooth follow camera
        /// </summary>
        public const float FollowSmoothTime = 0.25f;

        /// <summary>
        /// Default camera pitch
        /// </summary>
        public const int DefaultPitch = 55;

        /// <summary>
        /// Default zoom level
        /// </summary>
        public const int DefaultZoom = 8;

        /// <summary>
        /// Minimum zoom level
        /// </summary>
        public const int MinZoom = 1;

        /// <summary>
        /// Maximum zoom level
        /// </summary>
        public const int MaxZoom = 20;

        /// <summary>
        /// Field of view
        /// </summary>
        public const int FieldOfView = 25;

        [Header("General")]
        [SerializeField] private Camera _camera = null;
        [SerializeField] private Camera _logicCamera = null;
        [SerializeField] private Camera _wireCamera = null;
        [SerializeField] private GameObject _letterbox = null;

        [Header("Layers")]
        [SerializeField] [Layer] private int floorLayer = 0;
        [SerializeField] [Layer] private int staticLayer = 0;
        [SerializeField] [Layer] private int dynamicLayer = 0;
        [SerializeField] [Layer] private int logicLayer = 0;
        [SerializeField] [Layer] private int gizmoLayer = 0;
        [SerializeField] [Layer] private int fogLayer = 0;

        [Header("Background")]
        [SerializeField] private Background _defaultBackground = null;
        [SerializeField] private MeshRenderer _fog = null;

        private static CameraManager _instance = null;

        private Busy _busy = new Busy();
        private Background _background = null;
        private Transform _followTarget = null;
        private float _transitionElapsed = 0.0f;
        private float _transitionDuration = 0.0f;
        private AnimatedValue<float> _animatedPitch = new AnimatedValue<float>();
        private AnimatedValue<float> _animatedZoom = new AnimatedValue<float>();
        private AnimatedValue<Color> _animatedBackground = new AnimatedValue<Color>();
        private AnimatedValue<Vector3> _animatedTarget = new AnimatedValue<Vector3>();

        public static new Camera camera => _instance != null ? _instance._camera : null;

        /// <summary>
        /// Returns the default background 
        /// </summary>
        public static Background defaultBackground => _instance._defaultBackground;

        public static GameCamera.State baseCameraState { get; set; }
        public static GameCamera.State editorCameraState { get; set; }

        /// <summary>
        /// Get/Set the camera state
        /// </summary>
        public static CameraState state {
            get => new CameraState {
                valid = true,
                target = _instance._animatedTarget.to,
                pitch = (int)_instance._animatedPitch.to,
                background = _instance._background,
                zoom = (int)_instance._animatedZoom.to,
                followTarget = _instance._followTarget,
                cullingMask = _instance._camera.cullingMask,
                showLogicTiles = _instance._logicCamera.gameObject.activeSelf,
                showWires = _instance._wireCamera.gameObject.activeSelf,
                showFog = _instance._fog.gameObject.activeSelf,
                showLetterbox = _instance._letterbox.gameObject.activeSelf
            };
            set {
                if (!value.valid || _instance == null)
                    return;

                camera.cullingMask = value.cullingMask;
                _instance._logicCamera.gameObject.SetActive(value.showLogicTiles);
                _instance._wireCamera.gameObject.SetActive(value.showWires);
                _instance._fog.gameObject.SetActive(value.showFog);
                _instance._letterbox.gameObject.SetActive(value.showLetterbox);

                if (value.followTarget != null)
                    Follow(value.followTarget, value.pitch, value.zoom, value.background ?? _instance._defaultBackground, 0);
                else
                    Transition(value.target, value.pitch, value.zoom, value.background ?? _instance._defaultBackground, 0);
            }
        }

        public void Initialize()
        {
            _instance = this;
            _fog.material.color = Color.white;
            camera.fieldOfView = _logicCamera.fieldOfView = _wireCamera.fieldOfView = FieldOfView;
        }

        private void OnDisable()
        {
            _instance = null;
        }

        private void LateUpdate()
        {
            if (_instance == null)
                return;

            if (GameManager.puzzle == null)
                return;

            UpdateCameras();
            UpdateState();
        }

        private void UpdateBlendedState(ref GameCamera.State blendedState)
        {
            if (GameManager.puzzle == null)
                return;

            if (GameManager.puzzle.isEditing)
            {
                blendedState = editorCameraState;
                return;
            }

            SharedCameraData cameraData = GameManager.puzzle.GetSharedComponentData<SharedCameraData>(typeof(GameCamera));
            if (cameraData.activeCameras.Count == 0)
            {
                blendedState = baseCameraState;
                return;
            }

            blendedState = cameraData.activeCameras[0].state;
        }

        private void UpdateState()
        {
            GameCamera.State blendedState = new GameCamera.State();
            UpdateBlendedState(ref blendedState);

            // Change background color
            _instance._fog.material.color = blendedState.bgColor;

            // Update camera
            camera.transform.rotation = blendedState.rotation;
            camera.transform.position = blendedState.position;
        }

        /// <summary>
        /// Convert the given screen coordinate to a world coordinate
        /// </summary>
        /// <param name="screen">Screen coordinate</param>
        /// <returns>World coordinate</returns>
        public static Vector3 ScreenToWorld(Vector3 screen) => camera.ScreenToWorldPoint(screen);

        public static Vector2 WorldToScreen(Vector3 world) => camera.WorldToScreenPoint(world);

        private void TransitionInternal(Vector3 target, float pitch, int zoom, Background background, int transitionTime)
        {
            var targetBackground = background ?? _defaultBackground;

            // Clear out the Y component
            target = target.ToXZ();

            // Clamp zoom
            zoom = Mathf.Clamp(zoom, MinZoom, MaxZoom);

            _background = targetBackground;

            _transitionDuration = transitionTime * GameManager.tick;
            _transitionElapsed = 0.0f;

            // No animation
            if (transitionTime == 0)
            {
                _animatedBackground.Set(targetBackground.color);
                _animatedPitch.Set(pitch);
                _animatedTarget.Set(target);
                _animatedZoom.Set(zoom);

                // Update state immediately to ensure camera transforms are correct
                UpdateState();
            } 
            // Animation
            else
            {
#if true
                // Dont blow movement during follow camera transition?  
                // Disabled this for now because it forced you to make quick transisions or you lost control of your character
                _busy.enabled = _followTarget == null;
#else
                _busy.enabled = true;
#endif
                _animatedTarget.Set(_animatedTarget.value, target);
                _animatedPitch.Set(_animatedPitch.value, pitch);
                _animatedZoom.Set(_animatedZoom.value, zoom);
                _animatedBackground.Set(_animatedBackground.value, targetBackground.color);
            }
        }

        /// <summary>
        /// Transition camera from one state to another
        /// </summary>
        /// <param name="target">Target position of the camera</param>
        /// <param name="pitch">Pitch of the camera in degrees</param>
        /// <param name="zoomLevel">Zoom level to transition to</param>
        /// <param name="background">Background</param>
        /// <param name="transitionTime">Time to transition in ticks (0 for instant)</param>
        public static void Transition(Vector3 target, int pitch, int zoomLevel, Background background, int transitionTime) => 
            _instance.TransitionInternal(target, pitch, zoomLevel, background, transitionTime);

        /// <summary>
        /// Transition to a game camera
        /// </summary>
        /// <param name="gameCamera">Game camera to transition to</param>
        /// <param name="transitionTime">Optional transition time overload (-1 means use game camera transition time)</param>
        public static void Transition(GameCamera gameCamera, int transitionTime = -1) =>
            _instance.TransitionInternal (
                gameCamera.target, 
                gameCamera.pitch,
                gameCamera.zoomLevel, 
                gameCamera.background, 
                transitionTime == -1 ? gameCamera.transitionTime : transitionTime);

        public static void SetBackground (Background background)
        {
            _instance._animatedBackground.Set(background?.color ?? defaultBackground.color);
            _instance.UpdateState();
        }

#if false
        /// <summary>
        /// Transition to a different background
        /// </summary>
        /// <param name="background">New background</param>
        /// <param name="transitionTime">Time to transition</param>
        public static void Transition(Background background, int transitionTime) =>
            Transition(_instance._animatedTarget.value, _instance._zoomLevel, background, transitionTime);

        /// <summary>
        /// Adjust the active camera zoom by the given amount
        /// </summary>
        /// <param name="zoomLevel">delta zoom level</param>
        public static void Transition(int zoomLevel, int transitionTime) =>
            Transition(_instance._animatedTarget.value, zoomLevel, _instance._background, transitionTime);
#endif

        /// <summary>
        /// Pan the camera by the given amount in world coordinates
        /// </summary>
        /// <param name="pan"></param>
        public static void Pan(Vector3 pan) =>
            Transition(
                _instance._animatedTarget.value + pan, 
                (int)_instance._animatedPitch.value, 
                (int)_instance._animatedZoom.value, 
                _instance._background, 
                0);

        /// <summary>
        /// Pan the camera by the given amount in world coordinates
        /// </summary>
        /// <param name="pan"></param>
        public static void Zoom(int zoom) =>
            Transition(
                _instance._animatedTarget.value,
                (int)_instance._animatedPitch.value,
                zoom,
                _instance._background,
                0);

        /// <summary>
        /// Follow the given transform
        /// </summary>
        /// <param name="followTarget">Target transform to follow</param>
        /// <param name="pitch">Camera pitch</param>
        /// <param name="zoom">Camera zoom</param>
        /// <param name="background">Camera background</param>
        /// <param name="transitionTime">Time in game ticks to transition the camera values</param>
        public static void Follow(Transform followTarget, int pitch, int zoom, Background background, int transitionTime)
        {
            if (followTarget == null)
                return;

            // Set the follow target first to ensure we dont get put into a busy state when we do the transition
            _instance._followTarget = followTarget;

            // Start a normal transition for all properties 
            Transition(_instance._animatedTarget.value, pitch, zoom, background, transitionTime);

            // Reset the animated target for smoothing
            if(transitionTime == 0)
            {
                _instance._animatedTarget.from = Vector3.zero;
                _instance._animatedTarget.value = followTarget.position;
                _instance._animatedTarget.to = followTarget.position;
            }
        }

        /// <summary>
        /// Stop following a target
        /// </summary>
        public static void StopFollow()
        {
            if (_instance._followTarget == null)
                return;

            _instance._animatedTarget.Set(_instance._animatedTarget.value);
            _instance._followTarget = null;
        }

        /// <summary>
        /// Frame the camera on the given position using the given zoom level 
        /// </summary>
        /// <param name="pitch">Pitch of the camera</param>
        /// <param name="target">Target for the camera to focus on</param>
        /// <param name="zoom">Zoom level in number of vertical tiles that should be visible</param>
        /// <param name="fov">Camera fov</param>
        /// <returns></returns>
        public static Vector3 Frame(Vector3 target, float pitch, float zoom, float fov)
        {
            var frustumHeight = zoom;
            var distance = (frustumHeight * 0.5f) / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            return
                // Target position
                target

                // Zoom to frame entire target
                + (distance * -(Quaternion.Euler(pitch, 0, 0) * Vector3.forward));
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
        /// Hide or show fog
        /// </summary>
        public static void ShowFog(bool show = true)
        {
            if (show)
                camera.cullingMask |= (1 << _instance.fogLayer);
            else
                camera.cullingMask &= ~(1 << _instance.fogLayer);
        }

        /// <summary>
        /// Hide or show wires
        /// </summary>
        public static void ShowWires (bool show = true)
        {
            _instance._wireCamera.gameObject.SetActive(show);
        }

        /// <summary>
        /// Hide or show gizmos
        /// </summary>
        public static void ShowGizmos (bool show = true)
        {
            if (show)
                camera.cullingMask |= (1 << _instance.gizmoLayer);
            else
                camera.cullingMask &= ~(1 << _instance.gizmoLayer);
        }

        /// <summary>
        /// Hide or show gizmos
        /// </summary>
        public static void ShowLetterbox(bool show = true)
        {
            _instance._letterbox.gameObject.SetActive(show);
        }

        /// <summary>
        /// Show/Hide a tile layer
        /// </summary>
        /// <param name="layer">Tile layer to show/hide</param>
        /// <param name="show">True to show the layer, false to hide it</param>
        public static void ShowLayer(TileLayer layer, bool show = true)
        {
            if(layer == TileLayer.Logic)
            {
                _instance._logicCamera.gameObject.SetActive(show);
                return;
            }

            if (show)
                camera.cullingMask |= (1 << TileLayerToObjectLayer(layer));
            else
                camera.cullingMask &= ~(1 << TileLayerToObjectLayer(layer));
        }

        private void UpdateCameras()
        {
            SharedCameraData cameraData = GameManager.puzzle.GetSharedComponentData<SharedCameraData>(typeof(GameCamera));
            for (int i = cameraData.activeCameras.Count - 1; i >= 0; --i)
            {
                GameCamera gameCam = cameraData.activeCameras[i];
                gameCam.BlendUpdate();
                if (gameCam.isDead)
                    gameCam.RemoveCamera();
            }
        }
    }
}
