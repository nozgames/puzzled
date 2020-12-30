using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Cycle))]
    class StateCycle : TileComponent
    {
        private int stateIndex;

        [Editable(hidden = true)]
        public string[] steps { get; set; }

        [ActorEventHandler]
        private void OnCycleAdvance(CycleAdvanceEvent evt)
        {
            ++stateIndex;

            if (stateIndex >= steps.Length)
            {
                if (evt.isLooping)
                    stateIndex = 0;
                else
                    stateIndex = steps.Length - 1;
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
                bool isWireActive = ((tile.GetOutputOption(i, 0) & (1 << stateIndex)) != 0);
                tile.outputs[i].enabled = isWireActive;
            }
        }

        [ActorEventHandler]
        private void OnCycleReset(CycleResetEvent evt)
        {
            stateIndex = 0;
        }
    }
}
