using NoZ;

namespace Puzzled
{
    class Delay : TileComponent
    {
        [Editable]
        public int delayTicks { get; private set; }

        private bool isDelaying = false;
        private bool wasDelaying = false;
        private int tickCount = 0;

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnWirePowerChanged (WirePowerChangedEvent evt)
        {
            if (powerInPort.hasPower && !isDelaying)
            {
                tickCount = 0;
                isDelaying = true;
                wasDelaying = true;
            } 

            if(!powerInPort.hasPower && (isDelaying || wasDelaying))
            {
                tickCount = 0;
                isDelaying = false;
                wasDelaying = false;
                powerOutPort.SetPowered(false);
            }
        }

        [ActorEventHandler]
        private void OnTick (TickEvent evt) 
        {
            if (!isDelaying)
                return;

            if (wasDelaying)
                ++tickCount;

            wasDelaying = isDelaying;

            if (tickCount >= delayTicks)
            {
                powerOutPort.SetPowered(true);
                isDelaying = false;
            }
        }
    }
}
