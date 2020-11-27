using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PressurePlate : TileComponent
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
            tile.SetOutputsActive(true);
        }

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt)
        {
            pressed = false;
            UpdateVisuals();
            tile.SetOutputsActive(false);
        }

        private void UpdateVisuals()
        {
            visualPressed.SetActive(pressed);
            visualUnpressed.SetActive(!pressed);
        }
    }
}
