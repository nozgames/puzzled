using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class StaticCamera : TileComponent
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
                    CameraManager.Transition(_background, 0);
            }
        }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(SignalEvent))]
        public Port signalInPort { get; set; }

        private Background _background;
        private int _zoomLevel = CameraManager.DefaultZoomLevel;
        private int _transitionTime = 4;

        [SerializeField] private bool isInitialLocation = false;

        public bool isStartingCamera => isInitialLocation;

        [ActorEventHandler]
        private void OnSignal (SignalEvent evt)
        {
            if (isEditing)
                return;

            puzzle.SetActiveCamera(this, transitionTime);
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            if (isInitialLocation && (!isEditing || isLoading))
                puzzle.SetActiveCamera(this, 0);
        }
    }
}
