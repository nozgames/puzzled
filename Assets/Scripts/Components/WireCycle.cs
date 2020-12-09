using NoZ;
using UnityEngine;

namespace Puzzled
{
    class WireCycle : TileComponent
    {
        private int wireIndex;

        [Editable]
        public bool isCycling { get; set; } = true;

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            UpdateCyclingState();
        }

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt)
        {
            UpdateCyclingState();
        }

        [ActorEventHandler]
        private void OnTickStart(TickEvent evt)
        {
            if (!isCycling)
                return;

            if (tile.outputCount == 0)
                return;

            int oldWireIndex = wireIndex;
            wireIndex = (wireIndex + 1) % tile.outputCount;

            tile.SetOutputActive(oldWireIndex, false);
            tile.SetOutputActive(wireIndex, true);
        }

        private void UpdateCyclingState()
        {
            isCycling = tile.hasActiveInput;
        }
    }
}
