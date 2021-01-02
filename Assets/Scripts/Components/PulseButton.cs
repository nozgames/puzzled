using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PulseButton : TileComponent
    {
        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            tile.SendSignalToOutputs();
        }
    }
}
