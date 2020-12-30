using NoZ;
using UnityEngine;

namespace Puzzled
{
    class StateCycle : TileComponent
    {
        private int stateIndex;

        private bool isCycling = false;
        private bool wasCycling = false;

        [Editable]
        public bool clearOnDeactivate { get; set; } = true;

        [Editable]
        public bool isLooping { get; set; } = true;

        [Editable]
        public int ticksPerState { get; set; } = 1;

        private int _tickCount = 0;

        [Editable(hidden = true)]
        public string[] steps { get; set; }

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
        private void OnStart(StartEvent evt) => UpdateOutputWires();

        [ActorEventHandler]
        private void OnTickStart(TickEvent evt)
        {
            HandleTick();
        }

        private void HandleTick()
        {
            if (isTickFrameProcessed)
                return;

            if (!isCycling)
            {
                wasCycling = false;
                return;
            }

            isTickFrameProcessed = true;

            if (tile.outputCount == 0)
                return;

            if (wasCycling)
            {
                ++_tickCount;
                if (_tickCount < ticksPerState)
                    return;

                ++stateIndex;

                if (stateIndex >= steps.Length)
                {
                    if (isLooping)
                        stateIndex = 0;
                    else
                        stateIndex = steps.Length - 1;
                }
            }

            wasCycling = isCycling;

            UpdateOutputWires();
        }

        private void UpdateCyclingState()
        {
            isCycling = tile.hasActiveInput;

            if (isCycling)
            {
                HandleTick();
            }
            else
            {
                if (clearOnDeactivate)
                {
                    stateIndex = 0;
                    UpdateOutputWires();
                }

                _tickCount = 0;
            }
        }

        private void UpdateOutputWires()
        {
            if (!isCycling && clearOnDeactivate)
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
    }
}
