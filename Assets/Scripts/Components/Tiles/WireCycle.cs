using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Cycle))]
    class WireCycle : TileComponent
    {
        private int _wireIndex;

        /// <summary>
        /// Output port used to send the current cycle value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        private Port powerOutPort { get; set; }

        /// <summary>
        /// Output port used to send the current cycle value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        private Port valueOutPort { get; set; }

        [ActorEventHandler]
        private void OnCycleAdvance(CycleAdvanceEvent evt)
        {
            ++_wireIndex;

            if (_wireIndex >= powerOutPort.wireCount)
            {
                if (evt.isLooping)
                    _wireIndex = 0;
                else
                    _wireIndex = powerOutPort.wireCount - 1;
            }
        }

        [ActorEventHandler]
        private void OnCycleUpdate(CycleUpdateEvent evt)
        {
            valueOutPort.SendValue(_wireIndex + 1, true);

            if (!evt.isActive)
            {
                powerOutPort.SetPowered(false);
                return;
            }

            for (int i = 0; i < powerOutPort.wireCount; ++i)
                powerOutPort.SetPowered(i, _wireIndex == i);
        }

        [ActorEventHandler]
        private void OnCycleReset(CycleResetEvent evt)
        {
            _wireIndex = 0;
        }
    }
}
