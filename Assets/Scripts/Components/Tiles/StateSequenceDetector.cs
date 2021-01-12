using NoZ;
using UnityEngine;
using System.Collections.Generic;

namespace Puzzled
{
    class StateSequenceDetector : TileComponent
    {
        private int sequenceIndex = 0;

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        // TODO: trigger to reset?

        /// <summary>
        /// Output port used to send the current cycle value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        private Port valueOutPort { get; set; }

        [Editable(hidden = true)]
        public string[] steps { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => SetSequenceIndex(sequenceIndex);

        [ActorEventHandler]
        private void OnWirePowerChanged(WirePowerChangedEvent evt) => HandleWireChange();

        private void HandleWireChange()
        {
            for (int i = 0; i < powerInPort.wireCount; ++i)
            {
                bool isWireExpected = ((powerInPort.GetWireOption(i, 0) & (1 << sequenceIndex)) != 0);
                if (powerInPort.GetWire(i).hasPower != isWireExpected)
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
            ResetSequence();
        }

        private void ResetSequence()
        {
            SetSequenceIndex(0);
            powerOutPort.SetPowered(false);
        }

        private void HandleSequenceComplete()
        {
            powerOutPort.SetPowered(true);
        }

        private void SetSequenceIndex(int index)
        {
            sequenceIndex = index;
            if (steps != null && sequenceIndex >= steps.Length)
                HandleSequenceComplete();

            valueOutPort.SendValue(sequenceIndex + 1, true);
        }
    }
}
