using NoZ;

namespace Puzzled
{
    class TriggerBox : UsableTileComponent
    {
        // TODO: power in to use as a kill switch ? (no wires = self powered, else use power)

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        private Port powerOutPort {get; set;}

        public bool entered { get; private set; }

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            if (!isUsable)
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
            if (!isUsable)
                return;

            if (evt.isPlayer)
            {
                entered = false;
                powerOutPort.SetPowered(false);
            }
        }

        protected override void OnUsableChanged()
        {
            powerOutPort.SetPowered(false);
        }
    }
}
