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

        private Cell _cell = Cell.invalid;
        private int _zoomLevel;

        private static CameraManager _instance = null;

        private void Awake()
        {
            _zoomLevel = (int)_camera.orthographicSize;    
        }

        private void OnEnable()
        {
            _instance = this;
        }

        private void OnDisable()
        {
            _instance = null;
        }

        public static void JumpToTile(Cell cell, int zoomLevel)
        {
            _instance._camera.transform.position = TileGrid.CellToWorld(cell);
            _instance._camera.orthographicSize = zoomLevel / 2;

            _instance._cell = cell;
            _instance._zoomLevel = zoomLevel;
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
                tweenGroup.Child(Tween.Move(TileGrid.CellToWorld(_instance._cell), TileGrid.CellToWorld(cell), false));

            float duration = transitionTime * GameManager.tick;

            tweenGroup.Duration(duration)
                .OnStop(_instance.OnCameraTransitionComplete)
                .Start(_instance._camera.gameObject);

            _instance._cell = cell;
            _instance._zoomLevel = zoomLevel;
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
            if (_instance._cell == Cell.invalid)
            {
                if (GameManager.playerCell != Cell.invalid)
                    _instance._cell = GameManager.playerCell;
                else
                    _instance._cell = TileGrid.WorldToCell(_instance._camera.transform.position);
            }
        }

        public static void Stop()
        {

        }
    }
}
