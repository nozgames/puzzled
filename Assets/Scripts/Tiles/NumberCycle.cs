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

        /// <summary>
        /// Output port used to send the current cycle value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        private Port valueOutPort { get; set; }

        [ActorEventHandler]
        private void OnCycleAdvance(CycleAdvanceEvent evt)
        {
            if (values == null)
                return;

            ++valueIndex;

            if (valueIndex >= values.Length)
            {
                if (evt.isLooping)
                    valueIndex = 0;
                else
                    valueIndex = values.Length - 1;
            }
        }

        [ActorEventHandler]
        private void OnCycleUpdate(CycleUpdateEvent evt)
        {
            if (values == null || values.Length == 0)
                return;

            valueOutPort.SendValue(values[valueIndex], true);
        }

        [ActorEventHandler]
        private void OnCycleReset(CycleResetEvent evt)
        {
            valueIndex = 0;
        }
    }
}
