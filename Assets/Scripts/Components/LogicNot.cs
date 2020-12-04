using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicNot : TileComponent
    {
        [ActorEventHandler]
        private void OnActivateWire(ActivateWireEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnDeactivateWire(DeactivateWireEvent evt) => UpdateState();

        private void UpdateState()
        {
            tile.SetOutputsActive(!tile.hasActiveInput);
        }
    }
}
