using NoZ;
using UnityEngine;

namespace Puzzled
{
    class StateSelect : TileComponent
    {
        private int stateIndex;

        [Editable(hidden = true)]
        public string[] steps { get; set; }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            stateIndex = evt.wire.value;
            UpdateOutputWires();
        }

        [ActorEventHandler]
        private void OnWireValueChanged(WireValueChangedEvent evt)
        {
            stateIndex = evt.wire.value;
            UpdateOutputWires();
        }

        private void UpdateOutputWires()
        {
            for (int i = 0; i < tile.outputCount; ++i)
            {
                bool isWireActive = ((tile.GetOutputOption(i, 0) & (1 << stateIndex)) != 0);
                tile.outputs[i].enabled = isWireActive;
            }
        }
    }
}
