using NoZ;
using UnityEngine;

namespace Puzzled
{
    class TriggerBox : TileComponent
    {
        // TODO: power in to use as a kill switch ? (no wires = self powered, else use power)

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort {get; set;}

        public bool entered { get; private set; }

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            if (evt.isPlayer)
            {
                entered = true;
                powerOutPort.SetPowered(true);
            }
        }

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt)
        {
            if (evt.isPlayer)
            {
                entered = false;
                powerOutPort.SetPowered(false);
            }
        }
    }
}
