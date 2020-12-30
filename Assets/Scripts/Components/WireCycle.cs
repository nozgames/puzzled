using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Cycle))]
    class WireCycle : TileComponent
    {
        private int wireIndex;

        [ActorEventHandler]
        private void OnCycleAdvance(CycleAdvanceEvent evt)
        {
            ++wireIndex;

            if (wireIndex >= tile.outputCount)
            {
                if (evt.isLooping)
                    wireIndex = 0;
                else
                    wireIndex = tile.outputCount - 1;
            }
        }

        [ActorEventHandler]
        private void OnCycleUpdate(CycleUpdateEvent evt)
        {
            if (!evt.isActive)
            {
                tile.SetOutputsActive(false);
                return;
            }

            for (int i = 0; i < tile.outputCount; ++i)
            {
                bool isWireActive = (wireIndex == i);
                tile.outputs[i].enabled = isWireActive;
            }
        }

        [ActorEventHandler]
        private void OnCycleReset(CycleResetEvent evt)
        {
            wireIndex = 0;
        }
    }
}
