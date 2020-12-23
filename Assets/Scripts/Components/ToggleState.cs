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
        private void OnActivateWire(WireActivatedEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt) => UpdateState();

        private void UpdateState()
        {
            isOn = tile.hasActiveInput;
            UpdateVisuals();
            tile.SetOutputsActive(isOn);
        }

        private void UpdateVisuals()
        {
            visualOn.SetActive(isOn);
            visualOff.SetActive(!isOn);
        }
    }
}
