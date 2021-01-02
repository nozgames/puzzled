using NoZ;

namespace Puzzled
{
    // TODO: review how this works with new ports
    class ActivationLimit : TileComponent
    {
        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [Editable]
        public int limit { get; private set; } = 1;

        private int _activationCount = 0;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _activationCount = 0;
        }

        [ActorEventHandler]
        private void OnWirePower(WirePowerChangedEvent evt)
        {
            if (_activationCount >= limit)
                return;

            if (evt.hasPower)
            {
                ++_activationCount;
                powerOutPort.SetPowered(true);
            }
            else
            {
                powerOutPort.SetPowered(false);
            }
        }
    }
}
