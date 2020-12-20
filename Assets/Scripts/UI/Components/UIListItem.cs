using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Puzzled
{
    class UIListItem : MonoBehaviour, IPointerDownHandler
    {
        [Tooltip("Object to enable when the item is selected")]
        [SerializeField] private GameObject _selectedVisuals = null;

        private UIList list = null;
        private bool _selected = false;

        public UnityEvent<bool> onSelectionChanged = new UnityEvent<bool>();

        private void OnEnable()
        {
            UpdateVisuals();
            list = GetComponentInParent<UIList>();            
        }

        private void OnTransformParentChanged()
        {
            list = GetComponentInParent<UIList>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (null == list || eventData.button != PointerEventData.InputButton.Left)
                return;

            if(!selected)
                selected = true;
        }

        public bool selected {
            get => list != null && list.selected == transform.GetSiblingIndex();
            set {
                if (value == selected && value == _selected)
                    return;

                // If selected is being set by someone other than the list
                // then the selected value will not match so we should tell the list first
                if (value != selected)
                {
                    if (!value)
                        list.ClearSelection();
                    else
                        list.Select(transform.GetSiblingIndex());
                } 
                else if (value != _selected)
                {
                    _selected = value;
                    UpdateVisuals();
                    onSelectionChanged?.Invoke(selected);
                }
            }
        }

        private void UpdateVisuals()
        {
            _selectedVisuals.SetActive(_selected);
        }
    }
}
