using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PulseButton : TileComponent
    {
        [ActorEventHandler]
        private void OnQueryUse(QueryUseEvent evt)
        {
            evt.result = true;
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            tile.PulseOutputs();
        }
    }
}
