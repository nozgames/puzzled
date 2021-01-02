using NoZ;

namespace Puzzled
{
    // TODO: review how this works with new ports
    class ActivationLimit : TileComponent
    {
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true)]
        private Port signalInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Signal, legacy = true)]
        private Port signalOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(ResetSignal))]
        private Port resetPort { get; set; }

        [Editable]
        public int limit { get; private set; } = 1;

        private int _activationCount = 0;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _activationCount = 0;
        }

        [ActorEventHandler]
        private void OnTrigger(SignalEvent evt)
        {
            if (_activationCount >= limit)
                return;

            ++_activationCount;

            signalOutPort.SendSignal();
        }

        [ActorEventHandler]
        private void OnReset(ResetSignal evt) => _activationCount = 0;
    }
}
