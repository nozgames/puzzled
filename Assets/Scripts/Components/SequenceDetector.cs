using NoZ;
using UnityEngine;
using System.Collections.Generic;

namespace Puzzled
{
    class SequenceDetector : TileComponent
    {
        [Editable]
        public bool resetOnDeactivate { get; set; } 

        // FIXME: we need some sort of sequence data structure here to check against
        private int sequenceIndex = 0;
        private List<int> wireOrder;

        void Start()
        {
            // FIXME, we need to find some way to choose the wire order
            wireOrder = new List<int>();
            for (int i = 0; i < tile.inputCount; ++i)
                wireOrder.Add(i);
        }

        [ActorEventHandler]
        private void OnActivateWire(ActivateWireEvent evt)
        {
            if (tile.inputs[sequenceIndex] == evt.wire)
                HandleCorrectWire();
            else 
                HandleIncorrectWire();
        }

        [ActorEventHandler]
        private void OnDeactivateWire(DeactivateWireEvent evt)
        {
            // FIXME: should we allow for back-sequencing?
            if (resetOnDeactivate)
                Reset();
        }

        private void HandleCorrectWire()
        {
            ++sequenceIndex;
            if (sequenceIndex >= tile.inputCount)
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
