using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Puzzled.Editor;

namespace Puzzled.Editor
{
    public class UIInspectorFoldout : MonoBehaviour, IPointerClickHandler, IInspectorStateProvider
    {
        [SerializeField] private Graphic _graphicExpanded = null;
        [SerializeField] private Graphic _graphicCollapsed = null;
        [SerializeField] private Transform _content = null;

        [SerializeField] private bool _expanded = false;

        public bool expanded {
            get => _expanded;
            set {
                _expanded = value;
                UpdateVisuals();
            }
        }

        public string inspectorStateId {
            get {
                var parent = transform.GetComponentInParent<UIPropertyEditor>();
                if (parent == null)
                    return null;

                return $"{parent.target.id}_foldout";
            }
        }
            
        public object inspectorState {
            get => _expanded;
            set => expanded = (bool)value;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            expanded = !expanded;
        }

        private void OnEnable()
        {
            UpdateVisuals();
        }

#if UNITY_EDITOR
        private void OnValidate() => UpdateVisuals();
#endif

        private void UpdateVisuals()
        {
            if(_graphicCollapsed != null)
                _graphicCollapsed.gameObject.SetActive(!_expanded);

            if(_graphicExpanded != null)
                _graphicExpanded.gameObject.SetActive(_expanded);

            if(_content != null)
                _content.gameObject.SetActive(_expanded);
        }
    }
}
