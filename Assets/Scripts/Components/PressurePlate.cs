using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class PressurePlate : ActorComponent
    {
        [Header("Logic")]
        [SerializeField] private Port port;
        
        public bool pressed { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject visualPressed;
        [SerializeField] private GameObject visualUnpressed;

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            pressed = true;
            UpdateVisuals();
            port.FireActivate();
        }

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt)
        {
            pressed = false;
            UpdateVisuals();
            port.FireDeactivate();
        }

        private void UpdateVisuals()
        {
            visualPressed.SetActive(pressed);
            visualUnpressed.SetActive(!pressed);
        }
    }
}
