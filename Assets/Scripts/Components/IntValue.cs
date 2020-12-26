using NoZ;
using UnityEngine;

namespace Puzzled
{
    class IntValue : TileComponent
    {
        [Editable]
        public int value { get; set; }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            tile.SetOutputValue(value);
            tile.PulseOutputs();
        }
    }
}
