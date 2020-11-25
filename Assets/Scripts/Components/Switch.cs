using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Switch : PuzzledActorComponent
    {
        public bool isOn { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject visualOn;
        [SerializeField] private GameObject visualOff;

        [ActorEventHandler]
        private void OnQueryUse(QueryUseEvent evt)
        {
            evt.result = true;
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            if (isOn)
                TurnOn();
            else
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
