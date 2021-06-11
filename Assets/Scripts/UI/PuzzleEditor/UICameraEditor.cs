using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Puzzled.Editor
{
    public class UICameraEditor : MonoBehaviour, IScrollHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Camera _camera = null;
        [SerializeField] private CameraBoundsGizmo _boundsGizmo = null;
        [SerializeField] private Button _expandButton = null;
        [SerializeField] private Button _collapseButton = null;
        [SerializeField] private LayoutElement _view = null;

        private GameCamera _gameCamera;
        private bool _dragPan = false;
        private bool _dragPitch = false;
        private bool _dragCombine = false;
        private float _lastZoomTime = float.MinValue;
        private Vector2 _dragStart;
        private int _pitchStart;
        private Cell _offsetStart;
        private RectTransform _rectTransform;

        public GameCamera gameCamera {
            get => _gameCamera;
            set {
                _gameCamera = value;
                UpdateCamera();
            }
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _expandButton.onClick.AddListener(() => Expand());
            _collapseButton.onClick.AddListener(() => Collapse());

            Collapse();
        }

        private void OnEnable()
        {
            UpdateCamera();
        }

        private void OnDisable()
        {
            _camera.gameObject.SetActive(false);
            _boundsGizmo.gameObject.SetActive(false);
        }

        private void UpdateCamera()
        {
            _camera.gameObject.SetActive(gameObject.activeSelf &&  _gameCamera != null);
            _boundsGizmo.gameObject.SetActive(_camera.gameObject.activeSelf);
            if (!_camera.gameObject.activeSelf)
                return;

            _camera.transform.localEulerAngles = new Vector3(_gameCamera.pitch, _gameCamera.yaw, 0);
            _camera.transform.position = 
                CameraManager.Frame(
                    _gameCamera.target, 
                    _gameCamera.pitch,
                    _gameCamera.yaw,
                    _gameCamera.zoomLevel,
                    CameraManager.camera.fieldOfView
                    );

            _boundsGizmo.UpdateTransforms();
        }

        private void LateUpdate()
        {
            UpdateCamera();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (eventData.scrollDelta.y == 0)
                return;

            var zoomLevel = Mathf.Clamp(_gameCamera.zoomLevel + (eventData.scrollDelta.y > 0 ? -1 : 1), CameraManager.MinZoom, CameraManager.MaxZoom);
            if (zoomLevel == _gameCamera.zoomLevel)
                return;

            var combine = (Time.time - _lastZoomTime < 0.25f);
            UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(_gameCamera.tile, "zoomLevel", zoomLevel), combine);
            _lastZoomTime = Time.time;
            _dragCombine = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(_dragPitch)
            {
                var step = _rectTransform.rect.height * 0.9f / 90.0f;
                var dist = (eventData.position.y - _dragStart.y);
                var pitch = Mathf.Clamp(_pitchStart - (int)(dist / step), 0, 90);
                if(pitch != _gameCamera.pitch)
                {
                    UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(_gameCamera.tile, "pitch", pitch), _dragCombine);
                    UpdateCamera();
                    _dragCombine = true;
                }
                return;
            }

            if(_dragPan)
            {
                var step = _rectTransform.rect.height / (_gameCamera.zoomLevel * 4 + 1);
                var delta = (_dragStart - eventData.position) / step;

                var transformedDelta = _gameCamera.yawIndex switch
                {                    
                    0 => new Vector2Int((int)delta.x, (int)delta.y),
                    1 => new Vector2Int((int)delta.y, (int)-delta.x),
                    2 => new Vector2Int((int)-delta.x, (int)-delta.y),
                    3 => new Vector2Int((int)-delta.y, (int)delta.x),
                    _ => new Vector2Int((int)delta.x, (int)delta.y)
                };

                var offset = _offsetStart + transformedDelta;
                if (offset != _gameCamera.offset)
                {
                    UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(_gameCamera.tile, "offset", offset), _dragCombine);
                    UpdateCamera();
                    _dragCombine = true;
                }
                return;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _dragPitch = true;
                _pitchStart = (int)_camera.transform.localEulerAngles.x;
            } 
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _dragPan = true;
                _offsetStart = _gameCamera.offset;
            }

            _dragCombine = false;
            _dragStart = eventData.position;
        }

        private Vector3 ScreenToWorld(Vector2 position) => RayToWorld(_camera.ScreenPointToRay(position));

        private static Vector3 GetPointAtHeight(Ray ray, float height)
        {
            return ray.origin + (((ray.origin.y - height) / -ray.direction.y) * ray.direction);
        }

        private Vector3 RayToWorld (Ray ray)
        {
            if ((new Plane(Vector3.up, Vector3.zero)).Raycast(ray, out float enter))
                return ray.origin + ray.direction * enter;

            return Vector3.zero;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragPan = false;
            _dragCombine = false;
            _dragPitch = false;
        }

        public void Expand()
        {
            _expandButton.gameObject.SetActive(false);
            _collapseButton.gameObject.SetActive(true);
            _view.preferredWidth = _camera.targetTexture.width;
            _view.preferredHeight = _camera.targetTexture.height;
            _rectTransform.ForceUpdateRectTransforms();
        }

        public void Collapse()
        {
            _expandButton.gameObject.SetActive(true);
            _collapseButton.gameObject.SetActive(false);
            _view.preferredWidth = _camera.targetTexture.width * 0.5f;
            _view.preferredHeight = _camera.targetTexture.height * 0.5f;
            _rectTransform.ForceUpdateRectTransforms();
        }
    }
}
