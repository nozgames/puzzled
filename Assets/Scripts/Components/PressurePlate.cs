using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PressurePlate : PuzzledActorComponent
    {
        public bool pressed { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject visualPressed;
        [SerializeField] private GameObject visualUnpressed;

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            pressed = true;
            UpdateVisuals();
            actor.TriggerActivateWire();
        }

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt)
        {
            pressed = false;
            UpdateVisuals();
            actor.TriggerDeactivateWire();
        }

        private void UpdateVisuals()
        {
            visualPressed.SetActive(pressed);
            visualUnpressed.SetActive(!pressed);
        }
    }
}
