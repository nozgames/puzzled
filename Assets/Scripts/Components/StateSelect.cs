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
        public Port valueIn { get; set; }

        /// <summary>
        /// Output used to forward power to the current state
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOut { get; set; }

        /// <summary>
        /// Output used to send the current selected state index
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        public Port valueOut { get; set; }

        [ActorEventHandler]
        private void OnValueSignalEvent (ValueEvent evt) => UpdateOutput(evt.value);

        private void UpdateOutput(int value)
        {
            valueOut.SendValue(value);

            var stateIndex = value - 1;
            for (int i = 0; i < powerOut.wireCount; ++i)
                powerOut.SetPowered(i, (powerOut.GetWireOption(i, 0) & (1 << stateIndex)) != 0);
        }
    }
}
