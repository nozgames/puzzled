using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Door : TileComponent
    {
        private bool _open = false;

        [Editable]
        [SerializeField] private Tile keyItem = null;
        [SerializeField] private AudioClip _unlockSound = null;
        [SerializeField] private BlockRaycast _blockRaycast = null;

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [SerializeField] private Animator _animator = null;
        [SerializeField] private AudioClip _openSound = null;
        [SerializeField] private AudioClip _closeSound = null;
        [SerializeField] private GameObject _visualEast = null;
        [SerializeField] private GameObject _visualWest = null;

        [Editable]
        public bool isOpen {
            get => _open;
            set {
                _open = value;

                if (value)
                    _animator.SetTrigger((isLoading || isEditing) ? "Open" : "ClosedToOpen");
                else
                    _animator.SetTrigger((isLoading || isEditing) ? "Closed" : "OpenToClosed");

                if (value)
                    PlaySound(_openSound);
                else
                    PlaySound(_closeSound);

                if(null != _blockRaycast)
                    _blockRaycast.enabled = !_open;
            }
        }

        private bool requiresKey => keyItem != null;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateVisuals();

            if (null != _blockRaycast)
                _blockRaycast.enabled = !_open;
        }

        [ActorEventHandler(priority=1)]
        private void OnQueryMove(QueryMoveEvent evt)
        {
            evt.result = _open;
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            // Always report we were used, even if the use fails
            evt.IsHandled = true;

            if (_open)
                return;

            if (!requiresKey)
                return;

            // Check to see if the user has the item to unlock the chest
            bool shouldOpen = evt.user.Send(new QueryHasItemEvent(keyItem.guid));
            if (!shouldOpen)
                return;

            PlaySound(_unlockSound);

            isOpen = true;
        }

        [ActorEventHandler]
        private void OnWirePower (WirePowerChangedEvent evt)
        {
            // Do not let wires open/close locked doors
            if (requiresKey)
                return;

            if (powerInPort.hasPower && !isOpen)
                isOpen = true;
            else if (!powerInPort.hasPower && isOpen)
                isOpen = false;
        }

        [ActorEventHandler]
        private void CellChangedEvent(CellChangedEvent evt)
        {
            UpdateVisuals();
        }

        private Wall GetWall(Cell cell) => puzzle.grid.CellToComponent<Wall>(cell, TileLayer.Static);

        private void UpdateVisuals()
        {
            if (tile == null)
                return;

            var rotatedProperty = tile.GetProperty("rotated");
            var rotated = rotatedProperty?.GetValue<bool>(tile) ?? false;

            if (_visualWest != null)
                _visualWest.SetActive(GetWall(tile.cell + (rotated ? Cell.up : Cell.left)) != null);
            if (_visualEast != null)
                _visualEast.SetActive(GetWall(tile.cell + (rotated ? Cell.down : Cell.right)) != null);
        }
    }
}
