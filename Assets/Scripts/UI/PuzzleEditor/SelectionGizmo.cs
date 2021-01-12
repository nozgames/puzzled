using UnityEngine;

namespace Puzzled.Editor
{
    public class SelectionGizmo : MonoBehaviour
    {
        [SerializeField] private float _height = 1.0f;
        [SerializeField] private float _handleSize = 1.0f;
        [SerializeField] private float _lineSize = 1.0f;
        [SerializeField] private Transform _center = null;
        [SerializeField] private Transform[] _handles = null;
        [SerializeField] private Transform[] _lines = null;

        private Vector3 _min = new Vector3(-0.5f, 0, -0.5f);
        private Vector3 _max = new Vector3( 0.5f, 0,  0.5f);

        public Vector3 min {
            get => _min;
            set {
                if (_min == value)
                    return;
                _min = value;
                UpdateTransforms();
            }
        }

        public Vector3 max {
            get => _max;
            set {
                if (_max == value)
                    return;
                _max = value;
                UpdateTransforms();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateTransforms();
        }
#endif

        private void UpdateTransforms()
        {
            var size = max - min;
            var hsize = size * 0.5f;
            var center = min + hsize;

            transform.position = new Vector3(center.x, 0, center.z);

            _center.localPosition = new Vector3(0, _height * 0.5f, 0.0f);
            _center.localScale = new Vector3(size.x, Mathf.Max(0.01f,_height), size.z);

            _handles[0].transform.localPosition = new Vector3(-hsize.x, _height, -hsize.z);
            _handles[1].transform.localPosition = new Vector3( hsize.x, _height, -hsize.z);
            _handles[2].transform.localPosition = new Vector3(-hsize.x, _height,  hsize.z);
            _handles[3].transform.localPosition = new Vector3( hsize.x, _height,  hsize.z);
            _handles[4].transform.localPosition = new Vector3(-hsize.x,    0.0f, -hsize.z);
            _handles[5].transform.localPosition = new Vector3( hsize.x,    0.0f, -hsize.z);
            _handles[6].transform.localPosition = new Vector3(-hsize.x,    0.0f,  hsize.z);
            _handles[7].transform.localPosition = new Vector3( hsize.x,    0.0f,  hsize.z);

            var handleSize = Vector3.one * _handleSize;
            _handles[0].transform.localScale = handleSize;
            _handles[1].transform.localScale = handleSize;
            _handles[2].transform.localScale = handleSize;
            _handles[3].transform.localScale = handleSize;
            _handles[4].transform.localScale = handleSize;
            _handles[5].transform.localScale = handleSize;
            _handles[6].transform.localScale = handleSize;
            _handles[7].transform.localScale = handleSize;

            // Z-Axis
            _lines[0].transform.localPosition = new Vector3(-hsize.x, _height, 0.0f);
            _lines[0].transform.localScale = new Vector3(_lineSize, _lineSize, size.z);
            _lines[1].transform.localPosition = new Vector3(hsize.x, _height, 0.0f);
            _lines[1].transform.localScale = new Vector3(_lineSize, _lineSize, size.z);
            _lines[2].transform.localPosition = new Vector3(-hsize.x, 0.0f, 0.0f);
            _lines[2].transform.localScale = new Vector3(_lineSize, _lineSize, size.z);
            _lines[3].transform.localPosition = new Vector3(hsize.x, 0.0f, 0.0f);
            _lines[3].transform.localScale = new Vector3(_lineSize, _lineSize, size.z);

            // Z-Axis
            _lines[4].transform.localPosition = new Vector3(0.0f, _height, -hsize.z);
            _lines[4].transform.localScale = new Vector3(size.x, _lineSize, _lineSize);
            _lines[5].transform.localPosition = new Vector3(0.0f, _height, hsize.z);
            _lines[5].transform.localScale = new Vector3(size.x, _lineSize, _lineSize);
            _lines[6].transform.localPosition = new Vector3(0.0f, 0.0f, -hsize.z);
            _lines[6].transform.localScale = new Vector3(size.x, _lineSize, _lineSize);
            _lines[7].transform.localPosition = new Vector3(0.0f, 0.0f, hsize.z);
            _lines[7].transform.localScale = new Vector3(size.x, _lineSize, _lineSize);

            // Y-Axis
            _lines[8].transform.localPosition = new Vector3(-hsize.x, _height * 0.5f, -hsize.z);
            _lines[8].transform.localScale = new Vector3(_lineSize, _height, _lineSize);
            _lines[9].transform.localPosition = new Vector3(-hsize.x, _height * 0.5f, hsize.z);
            _lines[9].transform.localScale = new Vector3(_lineSize, _height, _lineSize);
            _lines[10].transform.localPosition = new Vector3(hsize.x, _height * 0.5f, -hsize.z);
            _lines[10].transform.localScale = new Vector3(_lineSize, _height, _lineSize);
            _lines[11].transform.localPosition = new Vector3(hsize.x, _height * 0.5f, hsize.z);
            _lines[11].transform.localScale = new Vector3(_lineSize, _height, _lineSize);
        }
    }
}
