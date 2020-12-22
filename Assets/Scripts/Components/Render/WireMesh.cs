using System;
using UnityEngine;

namespace Puzzled
{
    [Flags]
    public enum WireVisualState
    {
        Normal = 0,
        Bold = 1,
        Dark = 2,
        Selected = 4
    }

    public class WireMesh : MonoBehaviour
    {
        [SerializeField] private bool _tile = false;

        [SerializeField] private float _widthNormal = 0.1f;
        [SerializeField] private float _widthBold = 0.2f;

        [SerializeField] private Material _materialNormal = null;
        [SerializeField] private Material _materialNormalDark = null;
        [SerializeField] private Material _materialSelected = null;
        [SerializeField] private Material _materialSelectedDark = null;

        private MeshRenderer _renderer;
        private MeshFilter _filter;
        private Cell _target;
        private WireVisualState _state = WireVisualState.Normal;
        private float _width;

        public WireVisualState state {
            get => _state;
            set {
                if (_state == value)
                    return;

                var oldWidth = _width;
                _state = value;

                _width = (_state & WireVisualState.Bold) == WireVisualState.Bold ? _widthBold : _widthNormal;
                if (_width != oldWidth)
                    UpdateMesh();

                UpdateMaterial();
            }
        }

        public Cell target {
            get => _target;
            set {
                if (_target == value)
                    return;
                _target = value;
                UpdateMesh();
            }
        }

        private void Awake()
        {
            GetComponent<MeshRenderer>().sortingLayerName = "Wire";
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _width = _widthNormal;
            UpdateMesh();
        }

        private void UpdateMaterial()
        {
            if (_renderer == null)
                return;

            var materialState = _state & (WireVisualState.Selected | WireVisualState.Dark);
            if (materialState == (WireVisualState.Selected | WireVisualState.Dark))
                _renderer.sharedMaterial = _materialSelectedDark;
            else if (materialState == WireVisualState.Dark)
                _renderer.sharedMaterial = _materialNormalDark;
            else if (materialState == WireVisualState.Selected)
                _renderer.sharedMaterial = _materialSelected;
            else
                _renderer.sharedMaterial = _materialNormal;
        }

        private void OnEnable()
        {
            UpdateMaterial();
            UpdateMesh();
        }

        private void OnValidate()
        {
            UpdateMesh();
        }

        public void UpdateMesh()
        {
            if (_filter == null || _width <= 0.01f || !gameObject.activeSelf)
                return;

            var targetPosition = Puzzle.current.grid.CellToWorld(_target) - gameObject.transform.position;
            targetPosition.z = 0.0f;
            var hwidth = _width * 0.5f;
            var dir = targetPosition.normalized;
            var normal = Vector3.Cross(dir, Vector3.forward);
            var left = -normal * _width;
            var right = normal * _width;

            var length = targetPosition.magnitude - hwidth - _width;
            var sections = _tile ? Mathf.Min(Mathf.Max((int)Mathf.Ceil(length / _width), 1), 32) : 1;
            var sectionLength = (length / sections);
            var sectionSize = sectionLength;
            var sectionOffset = (sectionLength - sectionSize) * 0.5f;
            var vertCount = 4 + sections * 4;

            var verts = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];
            var triangles = new int[(sections + 2) * 6];

            // Left
            verts[0] = -dir * hwidth + left;
            verts[1] = targetPosition + dir * _width + left;
            verts[2] = -dir * hwidth + right;
            verts[3] = targetPosition + dir * _width + right;

            uvs[0] = new Vector2(1, 0);
            uvs[1] = new Vector2(0, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);

            triangles[0] = 0;
            triangles[1] = 4;
            triangles[2] = 6;

            triangles[3] = 0;
            triangles[4] = 6;
            triangles[5] = 2;

            triangles[6] = vertCount-3;
            triangles[7] = 1;
            triangles[8] = 3;

            triangles[9] = vertCount - 3;
            triangles[10] = 3;
            triangles[11] = vertCount - 1;

            var v = 4;
            var t = 12;
            var vleft = dir * hwidth + left;
            var vright = dir * hwidth + right;
            for (int i = 0; i < sections; i++, v += 4, t += 6)
            {
                var d = sectionLength * i + sectionOffset;
                verts[v + 0] = vleft + dir * d;
                verts[v + 1] = vleft + dir * (d + sectionSize);
                verts[v + 2] = vright + dir * d;
                verts[v + 3] = vright + dir * (d + sectionSize);
                uvs[v + 0] = new Vector2(0.75f, 0);
                uvs[v + 1] = new Vector2(0.5f, 0);
                uvs[v + 2] = new Vector2(0.75f, 1);
                uvs[v + 3] = new Vector2(0.5f, 1);
                triangles[t + 0] = v + 0;
                triangles[t + 1] = v + 1;
                triangles[t + 2] = v + 3;
                triangles[t + 3] = v + 0;
                triangles[t + 4] = v + 3;
                triangles[t + 5] = v + 2;
            }

            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            _filter.sharedMesh = mesh;            
        }
    }
}
