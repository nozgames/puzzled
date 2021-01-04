using NoZ;

namespace Puzzled
{
    class WireSelect : TileComponent
    {
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

        /// <summary>
        /// Output power port that poweres the wire matching the selectPort value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnValueSignal(ValueEvent evt) => UpdateOutputs(evt.value);

        private void UpdateOutputs(int value)
        {
            valueOutPort.SendValue(value);

            // Enable power for the selected wire and disabled for any other
            var wireIndex = value - 1;
            for (int i = 0; i < powerOutPort.wireCount; ++i)
                powerOutPort.SetPowered(i, (i == wireIndex));
        }
    }
}
