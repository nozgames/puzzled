using NoZ;
using UnityEngine;

namespace Puzzled
{
    class StaticCamera : TileComponent
    {
        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            ActivateCamera();
        }

        private void ActivateCamera()
        {
            CameraManager.TransitionToTile(tile.cell);
        }
    }
}
