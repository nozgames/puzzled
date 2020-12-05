using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicXAnd : TileComponent
    {
        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt) => UpdateState();

        private void UpdateState()
        {
            // FIXME: compare against wire config
            tile.SetOutputsActive(tile.allInputsActive);
        }
    }
}
