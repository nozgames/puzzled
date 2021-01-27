using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class CameraOrientationTest : MonoBehaviour
    {
        private Quaternion _rotation = Quaternion.identity;
        private bool _orthographic = false;
        private float _fov = 1.0f;

        void Update()
        {
            var camera = CameraManager.camera;
            if (camera == null)
                return;

            bool update = false;
            if(camera.transform.localRotation != _rotation)
            {
                _rotation = camera.transform.localRotation;
                update = true;
            }

            if (camera.fieldOfView != _fov)
            {
                _fov = camera.fieldOfView;
                update = true;
            }

            if(camera.orthographic != _orthographic)
            {
                _orthographic = camera.orthographic;
                update = true;
            }

            if (update)
                CameraManager.state = CameraManager.state;
        }
    }
}
