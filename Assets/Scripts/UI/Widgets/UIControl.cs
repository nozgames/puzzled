using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzled.UI
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public abstract class UIControl : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        protected const string StateNormal = "Normal";
        protected const string StateHover = "Hover";
        protected const string StateDisabled = "Disabled";

        [SerializeField] private bool _interactable = true;

        private bool _hover = false;
        private Animator _animator = null;

        public bool interactable {
            get => _interactable;
            set {
                _interactable = value;
                UpdateState();
            }
        }

        public bool hover => _hover;

        protected virtual void UpdateState()
        {
            if(!_interactable)
            {
                SetState(StateDisabled);
                return;
            }

            if(_hover)
            {
                SetState(StateHover);
                return;
            }

            SetState(StateNormal);
        }

        protected void SetState (string state)
        {
            if (_animator == null)
                return;

            _animator.SetTrigger(state);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hover = true;
            UpdateState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
            UpdateState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        protected override void Awake()
        {
            base.Awake();

            _animator = GetComponent<Animator>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateState();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public abstract string[] GetStates();
    }
}
