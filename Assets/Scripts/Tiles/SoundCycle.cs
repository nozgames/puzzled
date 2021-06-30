using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Cycle))]
    public class SoundCycle : TileComponent
    {
        private int valueIndex;

        [Editable]
        private Sound[] sounds { get; set; }

        /// <summary>
        /// Output port used to send the current cycle value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        private Port valueOutPort { get; set; }

        [ActorEventHandler]
        private void OnCycleAdvance(CycleAdvanceEvent evt)
        {
            ++valueIndex;

            if (valueIndex >= sounds.Length)
            {
                if (evt.isLooping)
                    valueIndex = 0;
                else
                    valueIndex = sounds.Length - 1;
            }
        }

        [ActorEventHandler]
        private void OnCycleUpdate(CycleUpdateEvent evt)
        {
            if (sounds == null || sounds.Length == 0)
                return;

            valueOutPort.SendValue(valueIndex, true);
            AudioManager.Instance.Play(sounds[valueIndex].clip);
        }

        [ActorEventHandler]
        private void OnCycleReset(CycleResetEvent evt)
        {
            valueIndex = 0;
        }
    }
}
