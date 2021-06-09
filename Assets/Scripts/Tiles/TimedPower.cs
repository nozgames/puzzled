using NoZ;

namespace Puzzled
{
    class TimedPower : TileComponent
    {
        [Editable]
        public int timeTicks { get; private set; }

        private bool isWaiting = false;
        private bool wasWaiting = false;
        private int tickCount = 0;

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true)]
        public Port signalInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnSignal(SignalEvent evt)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                wasWaiting = true;
            }

            tickCount = 0;
        }

        [ActorEventHandler]
        private void OnTick(TickEvent evt)
        {
            if (!isWaiting)
                return;

            if (wasWaiting)
                ++tickCount;

            wasWaiting = true;

            if (tickCount >= timeTicks)
                isWaiting = false;

            powerOutPort.SetPowered(isWaiting);
        }
    }
}
