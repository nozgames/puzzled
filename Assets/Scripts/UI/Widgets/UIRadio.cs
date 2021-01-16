using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Puzzled.UI
{
    public class UIRadio : UIControl, IPointerClickHandler
    {
        protected const string StateSelected = "Selected";

        [SerializeField] private bool _on = false;        
        [SerializeField] private UIRadioGroup _group = null;

        public UnityEvent<bool> onValueChanged = new UnityEvent<bool>();

        public bool isOn {
            get => _on;
            set {
                if (_on == value)
                    return;

                _on = value;
                UpdateState();

                if (_on && _group != null && IsActive())
                    _group.NotifyToggleOn(this);

                onValueChanged?.Invoke(_on);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            isOn = true;
        }

        protected override void UpdateState()
        {
            if (_on && IsActive())
            {
                SetState(StateSelected);
                return;
            }

            base.UpdateState();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetGroup(_group, false);
        }

        protected override void OnDisable()
        {
            SetGroup(null, false);
            base.OnDisable();
        }

        private void SetGroup(UIRadioGroup newGroup, bool setMemberValue)
        {
            var oldGroup = newGroup;

            if (oldGroup != null)
                oldGroup.UnregisterToggle(this);

            if (newGroup != null)
                newGroup.RegisterToggle(this);

            if (setMemberValue)
                _group = newGroup;

            // If we are in a new group, and this toggle is on, notify group.
            if (newGroup != null && newGroup != oldGroup && isOn && IsActive())
                newGroup.NotifyToggleOn(this);
        }

        public override string[] GetStates() =>
            new[] { StateNormal, StateDisabled, StateHover, StateSelected };
    }
}
