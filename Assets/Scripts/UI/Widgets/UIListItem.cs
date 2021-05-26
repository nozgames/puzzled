using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Puzzled.UI
{
    public class UIListItem : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [Tooltip("Object to enable when the item is selected")]
        [SerializeField] private GameObject _selectedVisuals = null;

        [Tooltip("Object to enable when the item is dragging")]
        [SerializeField] private GameObject _dragVisuals = null;

        [Tooltip("True if the item can be reordered")]
        [FormerlySerializedAs("_reoder")]
        [SerializeField] private bool _reorder = false;


        private UIList list = null;
        private bool _selected = false;
        private int _dragStart = -1;

        public UnityEvent<bool> onSelectionChanged = new UnityEvent<bool>();
        
        public UnityEvent onDoubleClick = new UnityEvent();

        protected virtual void Awake()
        {
            list = GetComponentInParent<UIList>();
        }

        protected virtual void OnEnable()
        {
            UpdateVisuals();
            list = GetComponentInParent<UIList>();            
        }

        private void OnTransformParentChanged()
        {
            list = GetComponentInParent<UIList>();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (null == list || eventData.button != PointerEventData.InputButton.Left)
                return;

            if(!selected)
                selected = true;
        }

        public bool selected {
            get => list != null && list.selected == transform.GetSiblingIndex();
            set {
                if(list == null)
                {
                    list = GetComponentInParent<UIList>();
                    if (list == null)
                        return;
                }

                if (value == selected && value == _selected)
                    return;

                // If selected is being set by someone other than the list
                // then the selected value will not match so we should tell the list first
                if (value != selected)
                {
                    if (!value)
                        list.ClearSelection();
                    else
                        list.SelectItem(transform.GetSiblingIndex());
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

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2 && eventData.button == PointerEventData.InputButton.Left)
            {
                onDoubleClick?.Invoke();
                list.OnDoubleClickItem(transform.GetSiblingIndex());
            }
        }

        private int PositionToItemIndex (PointerEventData eventData)
        {
            for (int i=0; i < list.transform.childCount; i++)
            {
                var rectTransform = list.transform.GetChild(i).GetComponent<RectTransform>();
                var rect = rectTransform.rect;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.enterEventCamera, out var point);
                if (point.x >= rect.min.x && point.x <= rect.max.x && point.y >= rect.min.y)
                    return i;
            }

            return -1;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_reorder)
                return;

            if(_dragVisuals != null)
                _dragVisuals.SetActive(true);

            _dragStart = transform.GetSiblingIndex();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_reorder)
                return;

            if (_dragVisuals != null)
                _dragVisuals.SetActive(false);

            int index = PositionToItemIndex(eventData);
            if (index == -1)
                index = _dragStart;

            transform.SetSiblingIndex(index);

            if(index != _dragStart)
                list.OnReorderItem(_dragStart, index);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_reorder)
                return;

            int index = PositionToItemIndex(eventData);
            if (index == -1)
                index = _dragStart;

            if (transform.GetSiblingIndex() != index)
                transform.SetSiblingIndex(index);
        }
    }
}
