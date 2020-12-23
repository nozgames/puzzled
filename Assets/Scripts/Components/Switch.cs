using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Switch : TileComponent
    {
        private bool _on = false;

        [Editable]
        public bool isOn {
            get => _on;
            set {
                if (_on == value)
                    return;

                _on = value;

                UpdateVisuals();

                if(tile != null)
                    tile.SetOutputsActive(_on);
            }
        }

        [Header("Visuals")]
        [SerializeField] private GameObject visualOn;
        [SerializeField] private GameObject visualOff;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateVisuals();
            tile.SetOutputsActive(_on);
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;

            ToggleSwitchState();
        }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            ToggleSwitchState();
        }

        private void ToggleSwitchState()
        {
            isOn = !isOn;
        }

        private void UpdateVisuals()
        {
            visualOn.SetActive(isOn);
            visualOff.SetActive(!isOn);
        }
    }
}
