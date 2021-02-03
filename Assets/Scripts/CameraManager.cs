﻿using NoZ;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class SharedCameraData
    {
        public List<GameCamera> activeCameras = new List<GameCamera>(16);
        public GameCamera.State baseCameraState;

        public void AddCamera(GameCamera cam)
        {
            int insertionLocation = 0;
            for (int i = 0; i < activeCameras.Count; ++i)
            {
                GameCamera activeCam = activeCameras[i];
                if (activeCam.priority == cam.priority)
                {
                    // deactivate other cameras on this priority
                    Debug.Assert(cam != activeCam);
                    activeCam.DeactivateCamera(cam.transitionTime);
                }
                else if (activeCameras[i].priority > cam.priority)
                {
                    insertionLocation = i;
                    break;
                }
            }

            activeCameras.Insert(insertionLocation, cam);
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
        [SerializeField] private GameObject _letterbox = null;

        [Header("Layers")]
        [SerializeField] [Layer] private int floorLayer = 0;
        [SerializeField] [Layer] private int staticLayer = 0;
        [SerializeField] [Layer] private int dynamicLayer = 0;
        [SerializeField] [Layer] private int logicLayer = 0;
        [SerializeField] [Layer] private int gizmoLayer = 0;
        [SerializeField] [Layer] private int wireLayer = 0;
        [SerializeField] [Layer] private int fogLayer = 0;

        [Header("Background")]
        [SerializeField] private Background _defaultBackground = null;
        [SerializeField] private MeshRenderer _fog = null;

        private static CameraManager _instance = null;

        private Busy _busy = new Busy();
        private Background _background = null;
        private Transform _followTarget = null;
        private AnimatedValue<float> _animatedPitch = new AnimatedValue<float>();
        private AnimatedValue<float> _animatedZoom = new AnimatedValue<float>();
        private AnimatedValue<Color> _animatedBackground = new AnimatedValue<Color>();
        private AnimatedValue<Vector3> _animatedTarget = new AnimatedValue<Vector3>();

        public static new Camera camera => _instance != null ? _instance._camera : null;

        /// <summary>
        /// Returns the default background 
        /// </summary>
        public static Background defaultBackground => _instance._defaultBackground;

        public static GameCamera.State editorCameraState { get; set; }


        public void Initialize()
        {
            _instance = this;
            _fog.material.color = Color.white;
            camera.fieldOfView = FieldOfView;
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

        private GameCamera.State GetBlendedCameraState()
        {
            if (GameManager.puzzle == null)
                return new GameCamera.State();

            if (GameManager.puzzle.isEditing)
                return editorCameraState;

            SharedCameraData cameraData = GameManager.puzzle.GetSharedComponentData<SharedCameraData>(typeof(GameCamera));
            GameCamera.State blendedState = cameraData.baseCameraState;

            float totalWeight = 1;
            for (int i = cameraData.activeCameras.Count - 1; i >= 0; --i)
            {
                GameCamera cam = cameraData.activeCameras[i];
                totalWeight += cam.weight;
                float lerpValue = cam.weight / totalWeight;
                blendedState = blendedState.Lerp(cameraData.activeCameras[i].state, lerpValue);
            }

            return blendedState;
        }

        private void UpdateState()
        {
            GameCamera.State blendedState = GetBlendedCameraState();

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

        public static void SetBackground (Background background)
        {
            _instance._fog.material.color = background?.color ?? defaultBackground.color;
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
