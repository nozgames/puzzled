using NoZ;
using UnityEngine;

namespace Puzzled
{
    class StaticCamera : TileComponent
    {
        [ActorEventHandler]
        private void OnActivateWire(ActivateWireEvent evt)
        {
            UpdateCameraState();
        }

        [ActorEventHandler]
        private void OnDeactivateWire(DeactivateWireEvent evt)
        {
            UpdateCameraState();
        }

        private void UpdateCameraState()
        {
            // FIXME
//            if (tile.hasActiveInput)
                // activate camera
//            else
                // deactivate camera
        }
    }
}
