using NoZ;
using UnityEngine;

namespace Puzzled
{
    class ToggleState : PuzzledActorComponent
    {
        public bool isOn { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject visualOn;
        [SerializeField] private GameObject visualOff;

        [ActorEventHandler]
        private void OnActivateWire(ActivateWireEvent evt)
        {
            TurnOn();
        }

        [ActorEventHandler]
        private void OnDeactivateWire(DeactivateWireEvent evt)
        {
            TurnOff();
        }

        private void TurnOn()
        {
            isOn = true;
            UpdateVisuals();
            actor.ActivateWire();
        }

        private void TurnOff()
        {
            isOn = false;
            UpdateVisuals();
            actor.DeactivateWire();
        }

        private void UpdateVisuals()
        {
            visualOn.SetActive(isOn);
            visualOff.SetActive(!isOn);
        }
    }
}
