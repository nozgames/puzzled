using UnityEngine;

namespace Puzzled
{
    public class WireVisuals : MonoBehaviour
    {
        [SerializeField] private Renderer _fromPortRenderer = null;
        [SerializeField] private Renderer _toPortRenderer = null;
        [SerializeField] private Renderer _wireRenderer = null;
        [SerializeField] private WireMesh _wireMesh = null;
        
        private PortType _portTypeFrom = PortType.Number;
        private PortType _portTypeTo = PortType.Number;        
        private Vector3 _target;
        private bool _selected = false;
        private bool _flowing = false;
        private bool _highlight = true;

        private void Awake()
        {
            _wireRenderer.sortingLayerName = "Wire";
            _fromPortRenderer.sortingLayerName = "Wire";
            _toPortRenderer.sortingLayerName = "Wire";

            highlight = _highlight;
            selected = _selected;
            flowing = _flowing;
        }

        private void OnEnable()
        {
            UpdateLayer();
        }

        private void UpdateLayer()
        {
            var z = 0;
            if(selected)
            {
                z = 100;
            }
            else
            {
                switch (_portTypeFrom)
                {
                    case PortType.Power: z = 0; break;
                    case PortType.Signal: z = 1; break;
                    case PortType.Number: z = 2; break;
                }
            }

            if (highlight)
                z += 12;

            _wireRenderer.sortingOrder = z;
            _fromPortRenderer.sortingOrder = z + 3;
            _toPortRenderer.sortingOrder = z + 6;
        }

        public PortType portTypeFrom {
            get => _portTypeFrom;
            set {
                _portTypeFrom = value;
                _wireRenderer.material.SetFloat("_portFrom", (float)value);
                _fromPortRenderer.material.SetFloat("_portType", (float)value);

                UpdateLayer();
            }
        }

        public PortType portTypeTo {
            get => _portTypeTo;
            set {
                _portTypeTo = value;
                _wireRenderer.material.SetFloat("_portTo", (float)value);
                _toPortRenderer.material.SetFloat("_portType", (float)value);
            }
        }

        public bool selected {
            get => _selected;
            set {
                _selected = value;
                var fvalue = value ? 1.0f : 0.0f;
                _wireRenderer.material.SetFloat("_selected", fvalue);
                _fromPortRenderer.material.SetFloat("_selected", fvalue);
                _toPortRenderer.material.SetFloat("_selected", fvalue);

                UpdateLayer();
            }                
        }

        public bool highlight {
            get => _highlight;
            set {
                _highlight = value;
                var fvalue = value ? 1.0f : 0.0f;
                _wireRenderer.material.SetFloat("_highlight", fvalue);
                _fromPortRenderer.material.SetFloat("_highlight", fvalue);
                _toPortRenderer.material.SetFloat("_highlight", fvalue);

                UpdateLayer();
            }
        }

        public bool flowing {
            get => _flowing;
            set {
                _flowing = value;
                var fvalue = value ? 1.0f : 0.0f;
                _wireRenderer.material.SetFloat("_flowing", fvalue);
            }
        }

        public Vector3 target {
            get => _target;
            set {
                _target = value;

                _wireMesh.target = target;
                _toPortRenderer.transform.parent.position = target;

                var dir = target - transform.position;
                if (dir.magnitude < 0.1f)
                    _toPortRenderer.transform.parent.rotation = Quaternion.LookRotation(-Vector3.right, Vector3.up);
                else
                    _toPortRenderer.transform.parent.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
        }
    }
}
