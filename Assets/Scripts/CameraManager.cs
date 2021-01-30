﻿using NoZ;
using UnityEngine;

namespace Puzzled
{
    /// <summary>
    /// Current state of the camera
    /// </summary>
    public struct CameraState
    {
        public bool valid;
        public Vector3 position;
        public Background background;
        public int zoomLevel;
        public bool editor;
        public Player followPlayer;
        public int cullingMask;
        public bool showLogicTiles;
        public bool showWires;
        public bool showFog;
    }

    /// <summary>
    /// Manages the camera
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        /// <summary>
        /// Default zoom level
        /// </summary>
        public const int DefaultZoomLevel = 8;

        /// <summary>
        /// Minimum zoom level
        /// </summary>
        public const int MinZoomLevel = 1;

        /// <summary>
        /// Maximum zoom level
        /// </summary>
        public const int MaxZoomLevel = 20;

        [Header("General")]
        [SerializeField] private Camera _camera = null;
        [SerializeField] private Camera _logicCamera = null;
        [SerializeField] private Camera _wireCamera = null;

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

        private Player _followPlayer = null;

        private static CameraManager _instance = null;

        public static new Camera camera => _instance != null ? _instance._camera : null;

        /// <summary>
        /// Returns the default background 
        /// </summary>
        public static Background defaultBackground => _instance._defaultBackground;

        /// <summary>
        /// Get/Set the camera state
        /// </summary>
        public static CameraState state {
            get => new CameraState {
                valid = true,
                position = _instance._targetPosition,
                background = _instance._background,
                zoomLevel = _instance._zoomLevel,
                followPlayer = _instance._followPlayer,
                cullingMask = _instance._camera.cullingMask,
                showLogicTiles = _instance._logicCamera.gameObject.activeSelf,
                showWires = _instance._wireCamera.gameObject.activeSelf,
                showFog = _instance._fog.gameObject.activeSelf
            };
            set {
                if (!value.valid || _instance == null)
                    return;

                camera.cullingMask = value.cullingMask;
                _instance._logicCamera.gameObject.SetActive(value.showLogicTiles);
                _instance._wireCamera.gameObject.SetActive(value.showWires);

                _instance._fog.gameObject.SetActive(value.showFog);

                if (value.followPlayer != null)
                    Follow(value.followPlayer, value.zoomLevel, value.background ?? _instance._defaultBackground, 0);
                else
                    Transition(value.position, value.zoomLevel, value.background ?? _instance._defaultBackground, 0);
            }
        }

        public void Initialize()
        {
            _instance = this;
            _fog.material.color = Color.white;
        }

        private void OnDisable()
        {
            _instance = null;
        }

        private void LateUpdate()
        {
            if (_followPlayer == null)
                return;

            camera.transform.position = Frame(_followPlayer.transform.position, camera.transform.localEulerAngles.x, _zoomLevel);
        }

        /// <summary>
        /// Convert the given screen coordinate to a world coordinate
        /// </summary>
        /// <param name="screen">Screen coordinate</param>
        /// <returns>World coordinate</returns>
        public static Vector3 ScreenToWorld(Vector3 screen) => camera.ScreenToWorldPoint(screen);

        private static bool LerpBackgroundColor(Tween tween, float t)
        {
            var lerped = tween.Param1 * (1f - t) + tween.Param2 * t;
            _instance._fog.material.color = new Color(lerped.x, lerped.y, lerped.z, 1.0f);
            return true;
        }

        private bool LerpOrthographicSize(Tween tween, float t)
        {
            camera.orthographicSize = Mathf.Lerp(tween.Param1.x, tween.Param1.y, t) / 2;
            return true;
        }

        /// <summary>
        /// Transition camera from one state to another
        /// </summary>
        /// <param name="position">Target position of the camera</param>
        /// <param name="zoomLevel">Zoom level to transition to</param>
        /// <param name="background">Background</param>
        /// <param name="transitionTime">Time to transition in ticks (0 for instant)</param>
        public static void Transition(Vector3 position, int zoomLevel, Background background, int transitionTime)
        {
            var targetBackground = background ?? _instance._defaultBackground;

            // Clear out the Y component
            position = Vector3.Scale(position, new Vector3(1, 0, 1));

            // Clamp zoom
            zoomLevel = Mathf.Clamp(zoomLevel, MinZoomLevel, MaxZoomLevel);

            // Handle non-animated transition
            if (transitionTime == 0)
            {
                if (camera.orthographic)
                    camera.orthographicSize = zoomLevel;

                // Change background color
                _instance._fog.material.color = targetBackground.color;

                Tween.Stop(_instance.gameObject, "Transition");

                camera.transform.position = Frame(position, camera.transform.localEulerAngles.x, zoomLevel);

                _instance._zoomLevel = zoomLevel;
                _instance._background = targetBackground;
                _instance._targetPosition = position;
                return;
            }

            var tweenGroup = Tween.Group();

            bool addedTweenChild = false;

            // Orthographic zoom
            if (camera.orthographic && camera.orthographicSize != zoomLevel)
            {
                tweenGroup.Child(Tween.Custom(_instance.LerpOrthographicSize, new Vector4(camera.orthographicSize, zoomLevel, 0), Vector4.zero));
                addedTweenChild = true;
            }

            // Move the camera if needed
            var targetPosition = Frame(position, camera.transform.localEulerAngles.x, zoomLevel);
            if (targetPosition != camera.transform.position)
            {
                tweenGroup.Child(Tween.Move(camera.transform.position, targetPosition, false).Target(camera.gameObject));
                addedTweenChild = true;
            }

            if (_instance._fog.material.color != targetBackground.color)
            {
                tweenGroup.Child(Tween.Custom(LerpBackgroundColor, _instance._fog.material.color, targetBackground.color));
                addedTweenChild = true;
            }

            if (addedTweenChild)
            {
                // Busy during a transition
                GameManager.busy++;

                tweenGroup
                    .Duration(transitionTime * GameManager.tick)
                    .EaseInOutCubic()
                    .Key("Transition")
                    .OnStop(_instance.OnTransitionComplete)
                    .Start(_instance.gameObject);
            }

            _instance._zoomLevel = zoomLevel;
            _instance._background = targetBackground;
            _instance._targetPosition = position;
        }

        /// <summary>
        /// Transition to a different background
        /// </summary>
        /// <param name="background">New background</param>
        /// <param name="transitionTime">Time to transition</param>
        public static void Transition(Background background, int transitionTime) =>
            Transition(_instance._targetPosition, _instance._zoomLevel, background, transitionTime);

        /// <summary>
        /// Adjust the active camera zoom by the given amount
        /// </summary>
        /// <param name="zoomLevel">delta zoom level</param>
        public static void Transition(int zoomLevel, int transitionTime) =>
            Transition(_instance._targetPosition, zoomLevel, _instance._background, transitionTime);

        /// <summary>
        /// Pan the camera by the given amount in world coordinates
        /// </summary>
        /// <param name="pan"></param>
        public static void Pan(Vector3 pan) =>
            Transition(_instance._targetPosition + pan, _instance._zoomLevel, _instance._background, 0);

        /// <summary>
        /// Called when a transition is complete to disable the busy state
        /// </summary>
        private void OnTransitionComplete() => GameManager.busy--;

        public static void StopFollow()
        {
            _instance._followPlayer = null;
        }

        public static void Follow(Player targetPlayer, int zoomLevel, Background background, int transitionTime)
        {
            if (targetPlayer == null)
                return;

            Transition(_instance._targetPosition, zoomLevel, background, transitionTime);
            _instance._followPlayer = targetPlayer;
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

        /// <summary>
        /// Frame the camera on the given position using the given zoom level 
        /// </summary>
        /// <param name="pitch">Pitch of the camera</param>
        /// <param name="target">Target for the camera to focus on</param>
        /// <param name="zoom">Zoom level in number of vertical tiles that should be visible</param>
        /// <returns></returns>
        public static Vector3 Frame (Vector3 target, float pitch, float zoom)
        {
            var frustumHeight = zoom;
            var cameraFov = camera.fieldOfView;
            var distance = (frustumHeight * 0.5f) / Mathf.Tan(cameraFov * 0.5f * Mathf.Deg2Rad);
            return
                // Target position
                target

                // Zoom to frame entire target
                + (distance *- (Quaternion.Euler(pitch, 0, 0) * Vector3.forward));
        }
    }
}
