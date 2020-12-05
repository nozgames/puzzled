using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicOr : TileComponent
    {
        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt) => UpdateState();

        private void UpdateState()
        {
            tile.SetOutputsActive(tile.hasActiveInput);
        }
    }
}
