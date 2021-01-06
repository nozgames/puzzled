using NoZ;

namespace Puzzled
{
    class LogicSpinner : TileComponent
    {
        private int _value = 0;

        [Editable]
        public int valueCount { get; private set; }

        // TODO: minvalue 
        // TODO: maxvalue

        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        public Port valueOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(IncrementSignal))]
        public Port incrementPort { get; set; }

        [ActorEventHandler]
        private void OnIncrement (IncrementSignal evt)
        {
            _value = (_value + 1) % valueCount;
            SendValue();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => SendValue();

        private void SendValue() => valueOutPort.SendValue(_value + 1, true);
    }
}
