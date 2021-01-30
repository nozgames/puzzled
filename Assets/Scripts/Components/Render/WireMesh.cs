using UnityEngine;

namespace Puzzled
{    
    public class WireMesh : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _filter;

        [Range(0,0.1f)]
        [SerializeField] private float _width = 0.1f;

        [Range(0.25f, 1.5f)]
        [SerializeField] private float _loopSize = 0.4f;

        private Vector3 _target;

        public Vector3 target {
            get => _target;
            set {
                _target = value;
                UpdateMesh();
            }
        }

        private void Awake()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            UpdateMesh();
        }

        private void OnEnable()
        {
            UpdateMesh();
        }

        private static readonly Vector3[] LoopDirs = {
            Vector3.forward,
            Vector3.right,
            -Vector3.forward,
            -Vector3.right
        };

        private void UpdateMeshLoop()
        {
            var verts = new Vector3[10];
            var uv0 = new Vector2[10];
            var uv1 = new Vector2[10];
            var triangles = new int[24];

            var hwidth = _width * 0.5f;
            var cornerWidth = hwidth / Mathf.Cos(45.0f * Mathf.Deg2Rad);
            var l = 0;
            var r = 1;
            var t = 0;

            verts[l] = -Vector3.right * hwidth;
            verts[r] = Vector3.right * hwidth;
            uv0[l] = new Vector2(0, 0);
            uv0[r] = new Vector2(0, 1);
            uv1[l] = new Vector2(0, 0);
            uv1[r] = new Vector2(0, 1);

            triangles[t + 0] = l + 0;
            triangles[t + 1] = l + 2;
            triangles[t + 2] = l + 1;
            triangles[t + 3] = l + 2;
            triangles[t + 4] = l + 3;
            triangles[t + 5] = l + 1;
            t += 6;
            l = 2;
            r = 3;

            var p = Vector3.zero + LoopDirs[0] * _loopSize;

            for (int i=1; i<4; i++, l+=2, r+=2, t+=6)
            {
                var dir = LoopDirs[i];
                var normal = Vector3.Cross(((LoopDirs[i-1] + dir) * 0.5f).normalized, -Vector3.up);
                verts[l] = p - normal * cornerWidth;
                verts[r] = p + normal * cornerWidth;
                uv0[l] = new Vector2(i * _loopSize, 0);
                uv0[r] = new Vector2(i * _loopSize, 1);
                uv1[l] = new Vector2(i / 4.0f, 0);
                uv1[r] = new Vector2(i / 4.0f, 1);
                triangles[t + 0] = l + 0;
                triangles[t + 1] = l + 2;
                triangles[t + 2] = l + 1;
                triangles[t + 3] = l + 2;
                triangles[t + 4] = l + 3;
                triangles[t + 5] = l + 1;

                p += dir * _loopSize;
            }

            verts[l] = -Vector3.forward * hwidth;
            verts[r] = Vector3.forward * hwidth;
            uv0[l] = new Vector2(_loopSize * 4, 0);
            uv0[r] = new Vector2(_loopSize * 4, 1);
            uv1[l] = new Vector2(1, 0);
            uv1[r] = new Vector2(1, 1);

            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.uv = uv0;
            mesh.uv2 = uv1;
            mesh.triangles = triangles;

            _filter.sharedMesh = mesh;
        }

        public void UpdateMesh()
        {
            if (_filter == null || _width <= 0.01f || !gameObject.activeSelf)
                return;

            var targetPosition = _target - gameObject.transform.position;
            targetPosition.y = 0.0f;
            var length = targetPosition.magnitude;

            if(length < 0.1f)
            {
                UpdateMeshLoop();
                return;
            }

            var dir = targetPosition.normalized;
            var hwidth = _width * 0.5f;
            var normal = Vector3.Cross(dir, -Vector3.up);
            var left = -normal * hwidth;
            var right = normal * hwidth;



            var verts = new Vector3[4];
            var uv0 = new Vector2[4];
            var uv1 = new Vector2[4];
            var normals = new Vector3[4];
            var triangles = new int[6];

            verts[0] = left;
            verts[1] = right;
            verts[2] = left + dir * length;
            verts[3] = right + dir * length;
            uv0[0] = new Vector2(0, 0);
            uv0[1] = new Vector2(0, 1);
            uv0[2] = new Vector2(length, 0);
            uv0[3] = new Vector2(length, 1);
            uv1[0] = new Vector2(0, 0);
            uv1[1] = new Vector2(0, 1);
            uv1[2] = new Vector2(1, 0);
            uv1[3] = new Vector2(1, 1);
            normals[0] = normals[1] = normals[2] = normals[3] = Vector3.up;

            triangles[0] = 0;
            triangles[1] = 2;
            triangles[2] = 1;
            triangles[3] = 2;
            triangles[4] = 3;
            triangles[5] = 1;

            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.uv = uv0;
            mesh.uv2 = uv1;
            mesh.triangles = triangles;
            mesh.normals = normals;

            _filter.sharedMesh = mesh;            
        }
    }
}
