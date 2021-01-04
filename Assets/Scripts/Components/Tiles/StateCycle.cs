using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Cycle))]
    class StateCycle : TileComponent
    {
        private int _stateIndex;

        /// <summary>
        /// Output port used to forward power to active states
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        /// <summary>
        /// Output port used to send the current cycle value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        public Port valueOutPort { get; set; }

        [Editable(hidden = true)]
        public string[] steps { get; set; }

        [ActorEventHandler]
        private void OnCycleAdvance(CycleAdvanceEvent evt)
        {
            if (steps == null || steps.Length == 0)
            {
                _stateIndex = -1;
                return;
            }

            ++_stateIndex;

            if (_stateIndex >= steps.Length)
            {
                if (evt.isLooping)
                    _stateIndex = 0;
                else
                    _stateIndex = steps.Length - 1;
            }
        }

        [ActorEventHandler]
        private void OnCycleUpdate(CycleUpdateEvent evt)
        {
            valueOutPort.SendValue(_stateIndex + 1);

            if (!evt.isActive)
            {
                powerOutPort.SetPowered(false);
                return;
            }

            for (int i = 0; i < powerOutPort.wireCount; ++i)
                powerOutPort.SetPowered(i, (powerOutPort.GetWireOption(i, 0) & (1 << _stateIndex)) != 0);
        }

        [ActorEventHandler]
        private void OnCycleReset(CycleResetEvent evt) => _stateIndex = 0;
    }
}
