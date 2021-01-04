using NoZ;

namespace Puzzled
{
    public class Select : TileComponent
    {
        private int _selectIndex = 1;

        /// <summary>
        /// Selected wire index
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Number, legacy = true)]
        public Port valueInPort { get; set; }

        /// Output power port that poweres the wire matching the selectPort value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        public Port valueOutPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateOutputs(_selectIndex);

        [ActorEventHandler]
        private void OnValueSignal(ValueEvent evt) => UpdateOutputs(evt.value);

        private void UpdateOutputs(int value)
        {
            _selectIndex = value;
            valueOutPort.SendValue(value);

            Send(new SelectUpdateEvent(this, value));
        }
    }
}
