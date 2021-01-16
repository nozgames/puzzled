using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Puzzled.UI
{
    public class UIToggle : UIControl, IPointerClickHandler
    {
        protected const string StateToggleOn = "ToggleOn";
        protected const string StateToggleOff = "ToggleOff";

        [SerializeField] private bool _on = false;
        
        public UnityEvent<bool> onValueChanged = new UnityEvent<bool>();

        public bool isOn {
            get => _on;
            set {
                if (_on == value)
                    return;

                _on = value;
                UpdateState();

                onValueChanged?.Invoke(_on);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            isOn = !isOn;
        }

        protected override void UpdateState()
        {
            SetState(_on ? StateToggleOn : StateToggleOff);

            if (hover && interactable)
                return;

            base.UpdateState();
        }

        public override string[] GetStates() => 
            new[] { StateNormal, StateDisabled, StateHover, StateToggleOn, StateToggleOff };
    }
}
