using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PressurePlate : TileComponent
    {
        private bool _pressed;

        [Header("Visuals")]
        [SerializeField] private GameObject visualPressed;
        [SerializeField] private GameObject visualUnpressed;

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt) => UpdateState();

        private void UpdateState()
        {
            // The pressure plate is switched if anything in the object layer is on the same tile
            var pressed = TileGrid.CellToTile(tile.cell, TileLayer.Object) != null;
            if (pressed == _pressed)
                return;

            _pressed = pressed;
            visualPressed.SetActive(pressed);
            visualUnpressed.SetActive(!pressed);
            tile.SetOutputsActive(pressed);
        }
    }
}
