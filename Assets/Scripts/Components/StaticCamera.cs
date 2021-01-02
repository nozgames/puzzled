using NoZ;
using UnityEngine;

namespace Puzzled
{
    class StaticCamera : TileComponent
    {
        [Editable]
        public int zoomLevel 
        { 
            get => _zoomLevel; 
            private set => _zoomLevel = Mathf.Clamp(value, CameraManager.MinZoomLevel, CameraManager.MaxZoomLevel); 
        }

        [Editable]
        public int transitionTime
        {
            get => _transitionTime;
            private set => _transitionTime = Mathf.Max(value, 0);
        }

        [Editable]
        public Background background {
            get => _background;
            set {
                if (_background == value)
                    return;

                _background = value;

                if(isInitialLocation)
                    CameraManager.TransitionToBackground(_background, 0);
            }
        }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(TriggerEvent))]
        public Port triggerPort { get; set; }

        private Background _background;
        private int _zoomLevel = CameraManager.DefaultZoomLevel;
        private int _transitionTime = 4;

        [SerializeField] private bool isInitialLocation = false;

        [ActorEventHandler]
        private void OnTrigger(TriggerEvent evt)
        { 
            if (isEditing)
                CameraManager.JumpToCell(tile.cell, zoomLevel);
            else
            {
                CameraManager.TransitionToCell(tile.cell, zoomLevel, transitionTime);
                CameraManager.TransitionToBackground(_background, transitionTime);
            }
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            if (isInitialLocation && !isEditing)
                CameraManager.JumpToCell(tile.cell, zoomLevel);

            if(isInitialLocation)
                CameraManager.TransitionToBackground(_background, 0);
        }
    }
}
