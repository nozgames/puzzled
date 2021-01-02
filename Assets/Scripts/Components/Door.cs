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


        [Header("Visuals")]
        [SerializeField] private GameObject openedVisual;
        [SerializeField] private GameObject closedVisual;

        [Editable]
        public bool isOpen {
            get => _open;
            set {
                _open = value;
                UpdateVisuals();
            }
        }

        private bool requiresKey => keyItem != System.Guid.Empty;

        private void Start()
        {
            UpdateVisuals();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _locked = requiresKey;
        }

        private void UpdateVisuals()
        {
            openedVisual.SetActive(_open);
            closedVisual.SetActive(!_open);
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
        private void OnWirePower (WirePowerEvent evt)
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
