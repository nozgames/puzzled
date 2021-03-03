using NoZ;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public partial class GameCamera
    {
        private class SharedCameraData
        {
            public List<GameCamera> activeCameras = new List<GameCamera>(16);
            public HashSet<GameCamera> cameraMap = new HashSet<GameCamera>();
            public GameCamera.State baseCameraState;
            private GameCamera _activeCamera = null;
            private int _activeLayer = -1;
            private int _lowestExpressedPriority = -1;

            public bool IsLayerExpressed(int layerIndex)
            {
                return layerIndex >= _lowestExpressedPriority;
            }

            public void ActivateCamera(GameCamera cam, float transitionTime)
            {
                // is this cam the new active?
                bool isInActiveLayer = false;
                if (cam.layer < _activeLayer)
                {
                    transitionTime = _activeCamera.remainingTransitionTime;

                    // this camera is not in active layer, snap to target if not expressed
                    if (IsLayerExpressed(cam.layer))
                    {
                        cam.SetBlendInTime(transitionTime);
                    }
                    else
                    {
                        cam.SnapToTargetWeight();
                        transitionTime = 0;
                    }
                }
                else
                {
                    _activeCamera = cam;
                    _activeLayer = cam.layer;
                    isInActiveLayer = true;

                    cam.state.isBusy = cam.busyDuringTransition;
                    cam.SetBlendInTime(transitionTime);
                }

                int insertionLocation = -1;
                for (int i = 0; i < activeCameras.Count; ++i)
                {
                    GameCamera activeCam = activeCameras[i];
                    if (activeCam == cam)
                        continue; // skip this camera if it is already in there

                    activeCam.state.isBusy = false;

                    if (activeCam.layer == cam.layer)
                    {
                        // deactivate other cameras on this layer
                        activeCam.SimpleDeactivateCamera(transitionTime);
                        continue;
                    }

                    // if the active cam is lower layer, put this cam earlier in list
                    if ((activeCam.layer <= cam.layer) && (insertionLocation < 0))
                        insertionLocation = i;

                    // update all other transitions to be in sync with this one if this one is active
                    if (isInActiveLayer && activeCam.isBlending)
                        activeCam.UpdateBlendRate(transitionTime);
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

            public void DeactivateCamera(GameCamera cam, float transitionTime)
            {
                if (cam.layer < _activeLayer)
                {
                    // this camera is not in active layer, snap to target if it isn't blending
                    if (cam.weight < 1)
                        cam.SetBlendOutTime(_activeCamera.remainingTransitionTime);
                    else
                        cam.SnapToTargetWeight();
                }
                else
                {
                    cam.state.isBusy = cam.busyDuringTransition;

                    // find next best active camera
                    for (int i = 0; i < activeCameras.Count; ++i)
                    {
                        GameCamera activeCam = activeCameras[i];
                        if (activeCam == cam)
                            continue;

                        _activeCamera = activeCam;
                        _activeLayer = activeCam.layer;
                        break;
                    }

                    cam.SetBlendOutTime(transitionTime);

                    // synchronize all other cameras to this transition time
                    for (int i = 0; i < activeCameras.Count; ++i)
                    {
                        GameCamera activeCam = activeCameras[i];
                        if (activeCam == cam)
                            continue; // this is the camera we are deactivating

                        if (activeCam.isBlending)
                            activeCam.UpdateBlendRate(transitionTime);
                    }
                }
            }

            public void RemoveCamera(GameCamera cam)
            {
                activeCameras.Remove(cam);
                cameraMap.Remove(cam);
            }

            public State UpdateCameraBlendingState()
            {
                // remove dead cameras
                for (int i = activeCameras.Count - 1; i >= 0; --i)
                {
                    GameCamera gameCam = activeCameras[i];
                    gameCam.BlendUpdate();
                    if (gameCam.isDead)
                        RemoveCamera(gameCam);
                }

                State blendedState = baseCameraState; // needs to be initialized to something
                State layerState = baseCameraState;

                float totalLayerWeight = 0;
                float layerWeight = 0;
                float visibleWeight = 1;
                int currentLayer = int.MaxValue;
                bool isCameraBusy = false;
                for (int i = 0; i < activeCameras.Count; ++i)
                {
                    GameCamera cam = activeCameras[i];

                    // update blending values
                    Debug.Assert(cam.weight > 0);
                    if (cam.layer < currentLayer)
                    {
                        // blend in previous layer
                        if (layerWeight > 0)
                        {
                            float scaledLayerWeight = Mathf.Min(1, layerWeight) * visibleWeight;

                            totalLayerWeight += scaledLayerWeight;
                            float layerLerpValue = scaledLayerWeight / totalLayerWeight;
                            blendedState.Lerp(layerState, layerLerpValue);

                            visibleWeight -= scaledLayerWeight;
                        }

                        if (visibleWeight <= 0)
                            break; // no more weight left

                        if (cam.state.isBusy)
                            isCameraBusy = true;

                        _lowestExpressedPriority = currentLayer;
                        currentLayer = cam.layer;
                        layerState = cam.state;
                        layerWeight = cam.weight;
                        continue; // not blending needed
                    }

                    layerWeight += cam.weight;
                    float lerpValue = cam.weight / layerWeight;
                    layerState.Lerp(cam.state, lerpValue);

                    if (cam.state.isBusy)
                        isCameraBusy = true;
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
                    float lerpValue = visibleWeight;
                    blendedState.Lerp(baseCameraState, lerpValue);
                }

                blendedState.isBusy = isCameraBusy;               
                return blendedState;
            }
        }
    }
}
