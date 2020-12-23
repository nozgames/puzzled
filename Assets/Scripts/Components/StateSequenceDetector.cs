using NoZ;
using UnityEngine;
using System.Collections.Generic;

namespace Puzzled
{
    class StateSequenceDetector : TileComponent
    {
        private int sequenceIndex = 0;

        [Editable(hidden = true)]
        public string[] steps { get; set; }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            HandleWireChange();
        }

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt)
        {
            HandleWireChange();
        }

        private void HandleWireChange()
        {
            for (int i = 0; i < tile.inputCount; ++i)
            {
                bool isWireExpected = ((tile.GetInputOption(i, 0) & (1 << sequenceIndex)) != 0);
                if (tile.inputs[i].enabled != isWireExpected)
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
            tile.SetOutputsActive(false);
        }

        private void HandleSequenceComplete()
        {
            tile.SetOutputsActive(true);
        }
    }
}
