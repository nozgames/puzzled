using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Usable))]
    class TriggerBox : TileComponent
    {
        private bool _isUsable = false;


        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        private Port powerOutPort {get; set;}

        public bool entered { get; private set; }

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            if (!_isUsable)
                return;

            if (evt.isPlayer)
            {
                entered = true;
                powerOutPort.SetPowered(true);
            }
        }

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt)
        {
            if (!_isUsable)
                return;

            if (evt.isPlayer)
            {
                entered = false;
                powerOutPort.SetPowered(false);
            }
        }

        [ActorEventHandler]
        private void OnUsableChanged(UsableChangedEvent evt)
        {
            _isUsable = evt.isUsable;
            powerOutPort.SetPowered(false);
        }
    }
}
