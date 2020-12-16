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

        [Editable]
        public int transitionTime
        {
            get => _transitionTime;
            private set => _transitionTime = Mathf.Max(value, 0);
        }

        private int _zoomLevel = 12;
        private int _transitionTime = 4;

        [SerializeField] private bool isInitialLocation = false;

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            ActivateCamera();
        }

        private void ActivateCamera()
        {
            CameraManager.TransitionToCell(tile.cell, zoomLevel, transitionTime);
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            if (isInitialLocation)
                CameraManager.JumpToTile(tile.cell, zoomLevel);
        }
    }
}
