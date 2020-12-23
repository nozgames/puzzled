using NoZ;
using UnityEngine;

namespace Puzzled
{
    class WireCycle : TileComponent
    {
        private int wireIndex;

        private bool isCycling = false;
        private bool wasCycling = false;

        [Editable]
        public bool clearOnDeactivate { get; set; } = true;

        [Editable]
        public bool isLooping { get; set; } = true;

        [Editable]
        public int ticksPerWire { get; set; } = 1;

        private int _tickCount = 0;

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
            if (!isCycling)
            {
                wasCycling = false;
                return;
            }

            if (tile.outputCount == 0)
                return;

            if (wasCycling)
            {
                ++_tickCount;
                if (_tickCount < ticksPerWire)
                    return;

                ++wireIndex;
                _tickCount = 0;

                if (wireIndex >= tile.outputCount)
                {
                    if (isLooping)
                        wireIndex = 0;
                    else
                        wireIndex = tile.outputCount - 1;
                }
            }

            wasCycling = isCycling;

            UpdateOutputWires();
        }

        private void UpdateCyclingState()
        {
            isCycling = tile.hasActiveInput;

            if (!isCycling)
            {
                if (clearOnDeactivate)
                {
                    wireIndex = 0;
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
                bool isWireActive = (wireIndex == i);
                tile.outputs[i].enabled = isWireActive;
            }
        }
    }
}
