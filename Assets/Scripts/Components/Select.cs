using NoZ;

namespace Puzzled
{
    public class Select : TileComponent
    {
        // this value is stored internally and only used for cases where a value is signaled
        private int _transientIndex = 0;

        /// <summary>
        /// Selected wire index
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Number, legacy = true)]
        public Port valueInPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateOutputs(_transientIndex);

        [ActorEventHandler]
        private void OnValueSignal(ValueEvent evt) 
        {
            if (!evt.wire.hasValue)
                _transientIndex = evt.value;

            UpdateOutputs(_transientIndex);
        }

        private void UpdateOutputs(int value)
        {
            Send(new SelectUpdateEvent(this, _transientIndex, valueInPort.wires));
        }
    }
}
