using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Checkpoint : TileComponent
    {
        [ActorEventHandler]
        private void OnActivateWire(ActivateWireEvent evt)
        {
            // FIXME
            // set respawn point
        }
    }
}
