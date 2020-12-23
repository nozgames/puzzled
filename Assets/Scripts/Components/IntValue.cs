using NoZ;
using UnityEngine;

namespace Puzzled
{
    class IntValue : TileComponent
    {
        [Editable]
        public int value { get; private set; }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            foreach (Wire wire in tile.outputs)
                wire.value = value;

            tile.PulseOutputs();
        }
    }
}
