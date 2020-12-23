using NoZ;
using UnityEngine;

namespace Puzzled
{
    class WireSelect : TileComponent
    {
        [SerializeField] private bool isProgressive = false;

        private int wireIndex;

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            wireIndex = evt.wire.value;
            UpdateOutputWires();
        }

        private void UpdateOutputWires()
        {
            for (int i = 0; i < tile.outputCount; ++i)
            {
                bool isActive = (i == wireIndex) || (isProgressive && (i < wireIndex));
                tile.SetOutputActive(i, isActive);
            }
        }
    }
}
