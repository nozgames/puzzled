using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Puzzled.UI
{
    public class UIListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("True if the item can be reordered")]
        [FormerlySerializedAs("_reoder")]
        [SerializeField] private bool _reorder = false;

        private Animator _animator = null;
        private UIList _list = null;
        private int _dragStart = -1;
        private bool _selected = false;
        private bool _hover = false;
        private bool _pressed = false;

        public UnityEvent<bool> onSelectionChanged = new UnityEvent<bool>();
        
        public UnityEvent onDoubleClick = new UnityEvent();

        public bool interactable { get; set; }

        public bool selected {
            get => _selected;
            set {
                if (value == _selected)
                    return;

                if(_list == null)
                {
                    _list = GetComponentInParent<UIList>();
                    if (_list == null)
                        return;
                }

                if (value)
                {
                    _list.ClearSelection();
                    _selected = true;
                } 
                else
                {
                    _selected = false;
                }

                UpdateAnimatorState();

                _list.OnSelectionChanged();
                onSelectionChanged?.Invoke(_selected);
            }
        }

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();

            _list = GetComponentInParent<UIList>();
        }

        protected virtual void OnEnable()
        {
            _list = GetComponentInParent<UIList>();

            UpdateAnimatorState();
        }

        protected virtual void OnDisable()
        {
            _hover = false;
            _pressed = false;
        }

        protected void OnTransformParentChanged()
        {
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

            _dragStart = transform.GetSiblingIndex();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_reorder)
                return;

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

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
            UpdateAnimatorState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            UpdateAnimatorState();
        }

        private void UpdateAnimatorState()
        {
            if (null == _animator)
                return;

            _animator.SetBool("hover", _hover);
            _animator.SetBool("pressed", _pressed);
            _animator.SetBool("selected", _selected);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            UpdateAnimatorState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;

            if (_hover && !selected)
                selected = true;
            else
                UpdateAnimatorState();
        }
    }
}
