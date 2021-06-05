using NoZ;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
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
        /// Default camera yaw
        /// </summary>
        public const int DefaultYaw = 0;

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
        [SerializeField] private GameObject _letterbox = null;

        [Header("Layers")]
        [SerializeField] [Layer] private int floorLayer = 0;
        [SerializeField] [Layer] private int staticLayer = 0;
        [SerializeField] [Layer] private int dynamicLayer = 0;
        [SerializeField] [Layer] private int logicLayer = 0;
        [SerializeField] [Layer] private int gizmoLayer = 0;
        [SerializeField] [Layer] private int wireLayer = 0;
        [SerializeField] [Layer] private int fogLayer = 0;
        [SerializeField] [Layer] private int wallLayer = 0;

        [Header("Background")]
        [SerializeField] private Background _defaultBackground = null;
        //[SerializeField] private MeshRenderer _fog = null;

        private static CameraManager _instance = null;

        public static new Camera camera => _instance != null ? _instance._camera : null;

        /// <summary>
        /// Returns the default background 
        /// </summary>
        public static Background defaultBackground => _instance._defaultBackground;

        public static GameCamera.State editorCameraState { get; set; }

        public static int yawIndex => _instance._blendedState.yawIndex;

        private GameCamera.State _blendedState;
        private bool _cameraIsBusy = false;


        public void Initialize()
        {
            _instance = this;
            //_fog.material.color = Color.white;
            camera.fieldOfView = FieldOfView;
        }

        private void OnDisable()
        {
            _instance = null;
        }

        public static void ForceUpdate() => _instance.LateUpdate();

        private void LateUpdate()
        {
            if (_instance == null)
                return;

            if (GameManager.puzzle == null)
                return;

            // update blended state
            _blendedState = (GameManager.puzzle.isEditing) ? editorCameraState : GameCamera.UpdateCameraBlendingState();

            if (_blendedState.isBusy != _cameraIsBusy)
            {
                _cameraIsBusy = _blendedState.isBusy;
                GameManager.busy += _cameraIsBusy ? 1 : -1;
            }

            camera.transform.position = CameraManager.Frame(_blendedState.targetPosition, _blendedState.pitch, _blendedState.yaw, _blendedState.zoomLevel, CameraManager.FieldOfView);
            camera.transform.rotation = Quaternion.Euler(_blendedState.pitch, _blendedState.yaw, 0);
        }

        /// <summary>
        /// Convert the given screen coordinate to a world coordinate
        /// </summary>
        /// <param name="screen">Screen coordinate</param>
        /// <returns>World coordinate</returns>
        public static Vector3 ScreenToWorld(Vector3 screen) => camera.ScreenToWorldPoint(screen);

        public static Vector2 WorldToScreen(Vector3 world) => camera.WorldToScreenPoint(world);

        public static void SetBackground (Background background)
        {
            //_instance._fog.material.color = background?.color ?? defaultBackground.color;
        }

        /// <summary>
        /// Frame the camera on the given position using the given zoom level 
        /// </summary>
        /// <param name="pitch">Pitch of the camera</param>
        /// <param name="yaw">Yaw of the camera</param>
        /// <param name="target">Target for the camera to focus on</param>
        /// <param name="zoom">Zoom level in number of vertical tiles that should be visible</param>
        /// <param name="fov">Camera fov</param>
        /// <returns></returns>
        public static Vector3 Frame(Vector3 target, float pitch, float yaw, float zoom, float fov)
        {
            var frustumHeight = zoom;
            var distance = (frustumHeight * 0.5f) / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            return
                // Target position
                target

                // Zoom to frame entire target
                + (distance * -(Quaternion.Euler(pitch, yaw, 0) * Vector3.forward));
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

                case TileLayer.Wall:
                case TileLayer.WallStatic:
                    return _instance.wallLayer;
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
            if (show)
                camera.cullingMask |= (1 << _instance.wireLayer);
            else
                camera.cullingMask &= ~(1 << _instance.wireLayer);
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
            if (show)
                camera.cullingMask |= (1 << TileLayerToObjectLayer(layer));
            else
                camera.cullingMask &= ~(1 << TileLayerToObjectLayer(layer));
        }
    }
}
