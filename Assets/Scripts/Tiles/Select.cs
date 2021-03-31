using NoZ;

namespace Puzzled
{
    public class Select : TileComponent
    {
        // this value is stored internally and only used for cases where a value is signaled
        private int _transientIndex = 0;

        public bool isPowered => ((powerInPort.wireCount == 0) || powerInPort.hasPower);

        [Editable]
        [Port(PortFlow.Input, PortType.Power)]
        public Port powerInPort { get; set; }

        /// <summary>
        /// Selected wire index
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Number, legacy = true)]
        public Port valueInPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateOutputs();

        [ActorEventHandler]
        private void OnValueSignal(ValueEvent evt) 
        {
            _transientIndex = evt.wire.hasValue ? -1 : evt.value;
            UpdateOutputs();
        }

        [ActorEventHandler]
        private void OnWirePowerChanged(WirePowerChangedEvent evt) => UpdateOutputs();

        private void UpdateOutputs()
        {
            Send(new SelectUpdateEvent(this, _transientIndex, valueInPort.wires));
        }
    }
}
