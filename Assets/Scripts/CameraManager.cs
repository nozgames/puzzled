using NoZ;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class SharedCameraData
    {
        public List<GameCamera> activeCameras = new List<GameCamera>(16);
        public HashSet<GameCamera> cameraMap = new HashSet<GameCamera>();
        public GameCamera.State baseCameraState;

        public void ActivateCamera(GameCamera cam, float transitionTime)
        {
            int insertionLocation = -1;
            for (int i = 0; i < activeCameras.Count; ++i)
            {
                GameCamera activeCam = activeCameras[i];
                if (activeCam == cam)
                    continue; // skip this camera if it is already in there

                if (activeCam.layer == cam.layer)
                {
                    // deactivate other cameras on this layer
                    Debug.Assert(cam != activeCam);
                    activeCam.DeactivateCamera(transitionTime);
                }
                else if (activeCam.layer <= cam.layer)
                {
                    // the active cam is lower layer, put this cam earlier in list
                    insertionLocation = i;
                    break;
                }
            }

            if (!cameraMap.Contains(cam))
            {
                if (insertionLocation >= 0)
                    activeCameras.Insert(insertionLocation, cam);
                else
                    activeCameras.Add(cam);

                cameraMap.Add(cam);
            }
        }

        public void RemoveCamera(GameCamera cam)
        {
            activeCameras.Remove(cam);
            cameraMap.Remove(cam);
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
            GameCamera.State blendedState = cameraData.baseCameraState; // needs to be initialized to something
            GameCamera.State layerState = cameraData.baseCameraState;

            float totalLayerWeight = 0;
            float layerWeight = 0;
            float visibleWeight = 1;
            int currentLayer = int.MaxValue;
            for (int i = 0; i < cameraData.activeCameras.Count; ++i)
            {
                GameCamera cam = cameraData.activeCameras[i];

                if (cam.layer < currentLayer)
                {
                    // blend in previous layer
                    if (layerWeight > 0)
                    {
                        float scaledLayerWeight = Math.Min(1, layerWeight) * visibleWeight;

                        totalLayerWeight += scaledLayerWeight;
                        float layerLerpValue = scaledLayerWeight / totalLayerWeight;
                        blendedState.Lerp(layerState, layerLerpValue);

                        visibleWeight -= scaledLayerWeight;
                        layerWeight = 0;
                    }

                    if (visibleWeight <= 0)
                        break; // done, no other priorities are visible

                    currentLayer = cam.layer;
                    layerState = cam.state;
                    layerWeight = cam.weight;
                    continue; // not blending needed
                }

                layerWeight += cam.weight;
                float lerpValue = cam.weight / layerWeight;
                layerState.Lerp(cam.state, lerpValue);
            }

            // blend in last layer if there is any weight
            if (layerWeight > 0)
            {
                float scaledLayerWeight = Math.Min(1, layerWeight) * visibleWeight;

                totalLayerWeight += scaledLayerWeight;
                float layerLerpValue = scaledLayerWeight / totalLayerWeight;
                blendedState.Lerp(layerState, layerLerpValue);

                visibleWeight -= scaledLayerWeight;
            }

            if (visibleWeight > float.Epsilon)
            {
                // blend in base state to fill visible weight
                float lerpValue = visibleWeight ;
                blendedState.Lerp(cameraData.baseCameraState, lerpValue);
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
