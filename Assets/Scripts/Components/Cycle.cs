using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Cycle : TileComponent
    {
        private bool isCycling = false;

        [Editable]
        public bool clearOnDeactivate { get; set; } = true;

        [Editable]
        public bool isLooping { get; set; } = true;

        [Editable]
        public int ticksPerWire { get; set; } = 1;

        private int _tickCount = 0;

        public bool shouldBeActive => isCycling || !clearOnDeactivate;

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => Send(new CycleUpdateEvent(this));

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
        private void OnTickStart(TickEvent evt)
        {
            UpdateCycle();
        }

        private void UpdateCycle()
        {
            if (isTickFrameProcessed)
                return; // already processed this tick

            if (!isCycling)
                return;

            isTickFrameProcessed = true;
            
            ++_tickCount;
            if (_tickCount < ticksPerWire)
                return;

            _tickCount = 0;

            Send(new CycleAdvanceEvent(this));
            Send(new CycleUpdateEvent(this));
        }

        private void UpdateCyclingState()
        {
            isCycling = tile.hasActiveInput;

            if (isCycling)
            {
                if (!tile.isTickFrame)
                    _tickCount = -1; // this will insure that the partial tick doesn't count

                Send(new CycleUpdateEvent(this));
            }
            else
            {
                if (clearOnDeactivate)
                {
                    Send(new CycleResetEvent(this));
                    Send(new CycleUpdateEvent(this));
                }

                _tickCount = 0;
            }
        }
    }
}
