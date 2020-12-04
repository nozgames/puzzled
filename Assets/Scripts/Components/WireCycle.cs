using NoZ;
using UnityEngine;

namespace Puzzled
{
    class WireCycle : TileComponent
    {
        private int wireIndex;
        private bool isCycling;

        [ActorEventHandler]
        private void OnActivateWire(ActivateWireEvent evt)
        {
            UpdateCyclingState();
        }

        [ActorEventHandler]
        private void OnDeactivateWire(DeactivateWireEvent evt)
        {
            UpdateCyclingState();
        }

        [ActorEventHandler]
        private void OnTickStart(TickEvent evt)
        {
            if (!isCycling)
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
