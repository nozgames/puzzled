using NoZ;

namespace Puzzled
{
    // TODO: review how this works with new ports
    class ActivationLimit : TileComponent
    {
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(TriggerEvent))]
        public Port triggerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Signal, legacy = true)]
        public Port triggerOutPort { get; set; }

        [Editable]
        public int limit { get; private set; } = 1;

        private int _activationCount = 0;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _activationCount = 0;
        }

        [ActorEventHandler]
        private void OnTrigger(TriggerEvent evt)
        {
            if (_activationCount >= limit)
                return;

            ++_activationCount;

            triggerOutPort.SendSignal();
        }
    }
}
