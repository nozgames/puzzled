using NoZ;

namespace Puzzled
{
    class SequenceDetector : TileComponent
    {
        private int sequenceIndex = 0;

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        // TODO: number out port for current sequence index
        // TODO: reset input signal port?

        [Editable(hidden = true)]
        public string[] steps { get; set; }
        
        [ActorEventHandler]
        private void OnWirePower (WirePowerChangedEvent evt)
        {
            if (!evt.hasPower)
                return;

            for (int i = 0; i < powerInPort.wireCount; ++i)
            {
                bool isWireExpected = ((powerInPort.GetWireOption(i, 0) & (1 << sequenceIndex)) != 0);
                if ((powerInPort.GetWire(i) == evt.wire) && !isWireExpected)
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
            ++sequenceIndex;
            if (sequenceIndex >= steps.Length)
                HandleSequenceComplete();
        }

        private void HandleIncorrectWire()
        {
            Reset();
        }

        private void Reset()
        {
            sequenceIndex = 0;
            powerOutPort.SetPowered(false);
        }

        private void HandleSequenceComplete()
        {
            powerOutPort.SetPowered(true);
        }
    }
}
