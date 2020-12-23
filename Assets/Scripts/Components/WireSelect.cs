using NoZ;
using UnityEngine;

namespace Puzzled
{
    class WireSelect : TileComponent
    {
        private int wireIndex;

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            wireIndex = evt.wire.value;
            UpdateOutputWires();
        }

        [ActorEventHandler]
        private void OnWireValueChanged(WireValueChangedEvent evt)
        {
            wireIndex = evt.wire.value;
            UpdateOutputWires();
        }

        private void UpdateOutputWires()
        {
            for (int i = 0; i < tile.outputCount; ++i)
            {
                bool isActive = (i == wireIndex);
                tile.SetOutputActive(i, isActive);
            }
        }
    }
}
