using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicXAnd : TileComponent
    {
        [ActorEventHandler]
        private void OnActivateWire(ActivateWireEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnDeactivateWire(DeactivateWireEvent evt) => UpdateState();

        private void UpdateState()
        {
            // FIXME: compare against wire config
            tile.SetOutputsActive(tile.allInputsActive);
        }
    }
}
