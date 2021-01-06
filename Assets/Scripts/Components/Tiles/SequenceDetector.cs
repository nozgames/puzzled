using NoZ;

namespace Puzzled
{
    class SequenceDetector : TileComponent
    {
        private int sequenceIndex = 0;

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true)]
        public Port signalInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        /// <summary>
        /// Output port used to send the current cycle value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        private Port valueOutPort { get; set; }

        // TODO: reset input signal port?

        [Editable(hidden = true)]
        public string[] steps { get; set; } = new string[0];

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => SetSequenceIndex(sequenceIndex);

        [ActorEventHandler]
        private void OnSignal (SignalEvent evt)
        {
            for (int i = 0; i < signalInPort.wireCount; ++i)
            {
                bool isWireExpected = ((signalInPort.GetWireOption(i, 0) & (1 << sequenceIndex)) != 0);
                if ((signalInPort.GetWire(i) == evt.wire) && !isWireExpected)
                {
                    // failure
                    HandleIncorrectWire();
                    return;
                }
            }

            HandleCorrectWire();
        }

        private void HandleCorrectWire()
        {
            SetSequenceIndex(sequenceIndex + 1);
        }

        private void HandleIncorrectWire()
        {
            Reset();
        }

        private void Reset()
        {
            SetSequenceIndex(0);
            powerOutPort.SetPowered(false);
        }

        private void SetSequenceIndex(int index)
        {
            sequenceIndex = index;
            if (sequenceIndex >= steps.Length)
                HandleSequenceComplete();

            valueOutPort.SendValue(sequenceIndex + 1, true);
        }

        private void HandleSequenceComplete()
        {
            powerOutPort.SetPowered(true);
        }
    }
}
