using NoZ;

namespace Puzzled
{
    class SequenceDetector : TileComponent
    {
        private int sequenceIndex = 0;

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(TriggerEvent))]
        public Port triggerPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        // TODO: number out port for current sequence index
        // TODO: reset input signal port?

        [Editable(hidden = true)]
        public string[] steps { get; set; }
        
        [ActorEventHandler]
        private void OnTrigger (TriggerEvent evt)
        {
            for (int i = 0; i < triggerPort.wireCount; ++i)
            {
                bool isWireExpected = ((triggerPort.GetWireOption(i, 0) & (1 << sequenceIndex)) != 0);
                if ((triggerPort.GetWire(i) == evt.wire) && !isWireExpected)
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
