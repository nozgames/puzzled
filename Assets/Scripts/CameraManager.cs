using NoZ;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzled
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private Camera _camera = null;
        [SerializeField] private float _transitionSpeed = 1;

        private static CameraManager _instance = null;

        private void OnEnable()
        {
            _instance = this;
        }

        private void OnDisable()
        {
            _instance = null;
        }

        public static void TransitionToTile(Cell cell)
        {
            GameManager.busy++;
           
            Tween.Move(_instance._camera.transform.position, TileGrid.CellToWorld(cell), false)
                    .Duration(_instance._transitionSpeed)
                    .OnStop(_instance.OnCameraTransitionComplete)
                    .Start(_instance._camera.gameObject);
        }

        private void OnCameraTransitionComplete()
        {
            GameManager.busy--;
        }
    }
}
