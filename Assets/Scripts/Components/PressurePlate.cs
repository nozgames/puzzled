using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PressurePlate : TileComponent
    {
        private bool _pressed;

        [Header("Visuals")]
        [SerializeField] private GameObject visualPressed = null;
        [SerializeField] private GameObject visualUnpressed = null;
        [SerializeField] private DecalSurface decalSurface = null;
        [SerializeField] private Vector3 decalOffsetPressed;
        [SerializeField] private Vector3 decalOffsetUnpressed;

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt) => UpdateState();

        private void UpdateState()
        {
            // The pressure plate is switched if anything in the dynamic layer is on the same tile
            var pressed = puzzle.grid.CellToTile(tile.cell, TileLayer.Dynamic) != null;
            
            _pressed = pressed;
            visualPressed.SetActive(pressed);
            visualUnpressed.SetActive(!pressed);
            powerOutPort.SetPowered(pressed);

            decalSurface.transform.localPosition = pressed ? decalOffsetPressed : decalOffsetUnpressed;
        }
    }
}
