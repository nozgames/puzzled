using NoZ;
using UnityEngine;

namespace Puzzled
{
    class ToggleState : TileComponent
    {
        public bool isOn { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject visualOn;
        [SerializeField] private GameObject visualOff;

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            TurnOn();
        }

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt)
        {
            TurnOff();
        }

        private void TurnOn()
        {
            isOn = true;
            UpdateVisuals();
            tile.SetOutputsActive(true);
        }

        private void TurnOff()
        {
            isOn = false;
            UpdateVisuals();
            tile.SetOutputsActive(false);
        }

        private void UpdateVisuals()
        {
            visualOn.SetActive(isOn);
            visualOff.SetActive(!isOn);
        }
    }
}
