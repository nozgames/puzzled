using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Usable))]
    class PressurePlate : TileComponent
    {
        private bool _pressed = false;
        private bool _isUsable = false;

        [Header("Visuals")]
        [SerializeField] private Animator _animator = null;
        [SerializeField] private AudioClip _upSound = null;
        [SerializeField] private AudioClip _downSound = null;

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateState(true, false);

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt) => UpdateState(false, true);

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt) => UpdateState(false, true);

        private void UpdateState(bool force=false, bool playSound = false)
        {
            // The pressure plate is switched if anything in the dynamic layer is on the same tile
            var pressed = puzzle.grid.CellToTile(tile.cell, TileLayer.Dynamic) != null;
            if (!force && pressed == _pressed)
                return;

            _pressed = pressed;

            if (playSound)
                PlaySound(_pressed ? _downSound : _upSound, 1.0f, _pressed ? 1.0f : 1.2f);

            _animator.SetTrigger(pressed ? "Down" : "Up");

            if (!_isUsable)
                powerOutPort.SetPowered(false);
            else
                powerOutPort.SetPowered(pressed);
        }

        [ActorEventHandler]
        private void OnUsableChanged(UsableChangedEvent evt)
        {
            _isUsable = evt.isUsable;
            powerOutPort.SetPowered(false);
        }
    }
}
