using UnityEngine;
using NoZ;

namespace Puzzled
{
    class BlockRaycast : TileComponent
    {
        [ActorEventHandler]
        private void OnRayCastEvent (RayCastEvent evt)
        {
            evt.hit = tile;
        }
    }
}
