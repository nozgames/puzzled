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
        [Port(PortFlow.Output, PortType.Power)]
        public Port powerOutPort { get; set; }

        /// <summary>
        /// Output port used to send the current cycle value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        public Port valuePort { get; set; }

        [Editable(hidden = true)]
        public string[] steps { get; set; }

        [ActorEventHandler]
        private void OnCycleAdvance(CycleAdvanceEvent evt)
        {
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
            valuePort.SendValue(_stateIndex + 1);

            if (!evt.isActive)
            {
                powerOutPort.SetPowered(false);
                return;
            }

            for (int i = 0; i < powerOutPort.wireCount; ++i)
                powerOutPort.SetPowered(i, (tile.GetOutputOption(i, 0) & (1 << _stateIndex)) != 0);
        }

        [ActorEventHandler]
        private void OnCycleReset(CycleResetEvent evt) => _stateIndex = 0;
    }
}
