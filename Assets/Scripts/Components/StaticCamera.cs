using NoZ;
using UnityEngine;

namespace Puzzled
{
    class StaticCamera : TileComponent
    {
        const int MinZoomLevel = 2;
        const int MaxZoomLevel = 20;

        [Editable]
        public int zoomLevel 
        { 
            get => _zoomLevel; 
            private set => _zoomLevel = Mathf.Clamp(value, MinZoomLevel, MaxZoomLevel); 
        }

        private int _zoomLevel = 12;

        [SerializeField] private bool isInitialLocation = false;

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            ActivateCamera();
        }

        private void ActivateCamera()
        {
            CameraManager.TransitionToCell(tile.cell, zoomLevel);
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            if (isInitialLocation)
                CameraManager.JumpToTile(tile.cell, zoomLevel);
        }
    }
}
