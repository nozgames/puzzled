using UnityEngine;

namespace Puzzled
{
    public class WireVisuals : MonoBehaviour
    {
        [SerializeField] private Renderer _fromPortRenderer = null;
        [SerializeField] private Renderer _toPortRenderer = null;
        [SerializeField] private Renderer _wireRenderer = null;

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

            _wireRenderer.material.renderQueue = 2000 + z;
            _fromPortRenderer.material.renderQueue = 2000 + z + 3;
            _toPortRenderer.material.renderQueue = 2000 + z + 6;
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
                var length = (value - transform.position).magnitude;
                length -= _toPortRenderer.transform.localScale.z;
                _wireRenderer.transform.localScale = new Vector3(_wireRenderer.transform.localScale.x, _wireRenderer.transform.localScale.y, length);
                _wireRenderer.material.SetFloat("_tiling", length * 4.0f);
                _toPortRenderer.transform.localPosition = new Vector3(0, 0, length);

                if(length > 0)
                    transform.rotation = Quaternion.LookRotation((value - transform.position).normalized);

                _target = value;
            }
        }
    }
}
