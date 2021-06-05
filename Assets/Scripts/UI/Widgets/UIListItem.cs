using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Puzzled.UI
{
    public class UIListItem : Selectable, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [Tooltip("Object to enable when the item is dragging")]
        [SerializeField] private GameObject _dragVisuals = null;

        [Tooltip("True if the item can be reordered")]
        [FormerlySerializedAs("_reoder")]
        [SerializeField] private bool _reorder = false;

        private UIList _list = null;
        private int _dragStart = -1;

        public UnityEvent<bool> onSelectionChanged = new UnityEvent<bool>();
        
        public UnityEvent onDoubleClick = new UnityEvent();

        public bool selected {
            get => currentSelectionState == SelectionState.Selected;
            set => Select();
        }

        protected override void Awake()
        {
            base.Awake();

            _list = GetComponentInParent<UIList>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _list = GetComponentInParent<UIList>();            
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            _list = GetComponentInParent<UIList>();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2 && eventData.button == PointerEventData.InputButton.Left)
            {
                onDoubleClick?.Invoke();
                _list.OnDoubleClickItem(transform.GetSiblingIndex());
            }
        }

        private int PositionToItemIndex (PointerEventData eventData)
        {
            for (int i=0; i < _list.transform.childCount; i++)
            {
                var rectTransform = _list.transform.GetChild(i).GetComponent<RectTransform>();
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
                _list.OnReorderItem(_dragStart, index);
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

        public override void OnSelect (BaseEventData eventData)
        {
            base.OnSelect(eventData);

            onSelectionChanged?.Invoke(true);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            onSelectionChanged?.Invoke(false);
        }
    }
}
