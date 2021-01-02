using NoZ;

namespace Puzzled
{
    class StateSelect : TileComponent
    {
        [Editable(hidden = true)]
        public string[] steps { get; set; }

        /// <summary>
        /// Input port used to set current selected state
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Number, legacy = true)]
        public Port selectPort { get; set; }

        /// <summary>
        /// Output used to forward power to the current state
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        /// <summary>
        /// Output used to send the current selected state index
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        public Port valuePort { get; set; }

        [ActorEventHandler]
        private void OnValueSignalEvent (ValueEvent evt) => UpdateOutput(evt.value);

        private void UpdateOutput(int value)
        {
            valuePort.SendValue(value);

            var stateIndex = value - 1;
            for (int i = 0; i < powerOutPort.wireCount; ++i)
                powerOutPort.SetPowered(i, (powerOutPort.GetWireOption(i, 0) & (1 << stateIndex)) != 0);
        }
    }
}
