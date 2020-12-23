using NoZ;
using UnityEngine;

namespace Puzzled
{
    class ActivationLimit : TileComponent
    {
        [Editable]
        public int limit { get; private set; } = 1;

        private int _activationCount = 0;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _activationCount = 0;
        }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            if (_activationCount >= limit)
                return;

            ++_activationCount;
            tile.SetOutputsActive(true);
        }

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt)
        {
            if (_activationCount >= limit)
                return;

            tile.SetOutputsActive(false);
        }
    }
}
