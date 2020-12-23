using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Delay : TileComponent
    {
        [Editable]
        public int delayTicks { get; private set; }

        private bool isDelaying = false;
        private bool wasDelaying = false;
        private int tickCount = 0;

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            tickCount = 0;
            isDelaying = true;
            wasDelaying = true;
        }

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt)
        {
            tickCount = 0;
            isDelaying = false;
            wasDelaying = false;

            tile.SetOutputsActive(false);
        }

        [ActorEventHandler]
        private void OnTickStart(TickEvent evt) 
        {
            if (!isDelaying)
                return;

            if (wasDelaying)
                ++tickCount;

            wasDelaying = isDelaying;

            if (tickCount >= delayTicks)
            {
                tile.SetOutputsActive(true);
                isDelaying = false;
            }
        }
    }
}
