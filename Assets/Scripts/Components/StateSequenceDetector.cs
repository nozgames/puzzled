using NoZ;
using UnityEngine;
using System.Collections.Generic;

namespace Puzzled
{
    class StateSequenceDetector : TileComponent
    {
        private int sequenceIndex = 0;

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        // TODO: trigger to reset?

        // TODO: number out port for current state?

        [Editable(hidden = true)]
        public string[] steps { get; set; }

        [ActorEventHandler]
        private void OnWirePower(WirePowerChangedEvent evt) => HandleWireChange();

        private void HandleWireChange()
        {
            for (int i = 0; i < powerInPort.wireCount; ++i)
            {
                bool isWireExpected = ((powerInPort.GetWireOption(i, 0) & (1 << sequenceIndex)) != 0);
                if (powerInPort.GetWire(i).isPowered != isWireExpected)
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
            ResetSequence();
        }

        private void ResetSequence()
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
