using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Cycle))]
    public class NumberCycle : TileComponent
    {
        private int valueIndex;

        [Editable]
        public int[] values {
            get;
            set;
        }

        [ActorEventHandler]
        private void OnCycleAdvance(CycleAdvanceEvent evt)
        {
            ++valueIndex;

            if (valueIndex >= values.Length)
            {
                if (evt.isLooping)
                    valueIndex = 0;
                else
                    valueIndex = tile.outputCount - 1;
            }
        }

        [ActorEventHandler]
        private void OnCycleUpdate(CycleUpdateEvent evt)
        {
            Debug.Assert(valueIndex < values.Length);
            tile.SetOutputValue(values[valueIndex]);
            tile.SetOutputsActive(true);
        }

        [ActorEventHandler]
        private void OnCycleReset(CycleResetEvent evt)
        {
            valueIndex = 0;
        }
    }
}
