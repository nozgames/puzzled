using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Checkpoint : TileComponent
    {
        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            // FIXME
            // set respawn point
        }
    }
}
