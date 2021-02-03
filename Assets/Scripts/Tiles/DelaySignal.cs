using NoZ;

namespace Puzzled
{
    class DelaySignal : TileComponent
    {
        [Editable]
        public int delayTicks { get; private set; }

        [Editable]
        public bool triggerResetsDelay { get; private set; } = false;

        private bool isDelaying = false;
        private bool wasDelaying = false;
        private int tickCount = 0;

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true)]
        public Port signalInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Signal, legacy = true)]
        public Port signalOutPort { get; set; }

        [ActorEventHandler]
        private void OnSignal(SignalEvent evt)
        {
            if (isDelaying)
            {
                if (!triggerResetsDelay)
                    return; // nothing to do, delay already in progress
            }
            else 
            {
                isDelaying = true;
                wasDelaying = true;
            }

            tickCount = 0;
        }

        [ActorEventHandler]
        private void OnTick(TickEvent evt)
        {
            if (!isDelaying)
                return;

            if (wasDelaying)
                ++tickCount;

            wasDelaying = true;

            if (tickCount >= delayTicks)
            {
                signalOutPort.SendSignal();
                isDelaying = false;
            }
        }
    }
}
