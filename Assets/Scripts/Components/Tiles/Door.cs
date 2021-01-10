using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Door : TileComponent
    {
        private bool _open = false;
        private bool _locked = false;

        [Editable]
        public System.Guid keyItem { get; private set; } = System.Guid.Empty;

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [SerializeField] private Animator _animator = null;
        [SerializeField] private AudioClip _openSound = null;
        [SerializeField] private AudioClip _closeSound = null;

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
            }
        }

        private bool requiresKey => keyItem != System.Guid.Empty;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _locked = requiresKey;
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

            if (!requiresKey)
                return;

            if (_locked)
            {
                // check if the using actor has the keyItem
                _locked = evt.user.Send(new QueryHasItemEvent(keyItem));
            }

            // Open if no longer locked
            if (!_locked)
                isOpen = true;            
        }

        [ActorEventHandler]
        private void OnWirePower (WirePowerChangedEvent evt)
        {
            if(powerInPort.hasPower && !isOpen)
            {
                // Do not let wires open locked doors
                if (_locked)
                    return;

                isOpen = true;
            }
            else if(!powerInPort.hasPower && isOpen)
            {
                isOpen = false;
            }
        }
    }
}
