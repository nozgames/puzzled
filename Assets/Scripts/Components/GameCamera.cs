using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class GameCamera : TileComponent
    {
        [Editable(rangeMin = CameraManager.MinZoomLevel, rangeMax = CameraManager.MaxZoomLevel)]
        public int zoomLevel
        {
            get => _zoomLevel;
            set => _zoomLevel = Mathf.Clamp(value, CameraManager.MinZoomLevel, CameraManager.MaxZoomLevel);
        }

        [Editable(rangeMin = 25, rangeMax = 90)]
        public int pitch {
            get => _pitch;
            set => _pitch = Mathf.Clamp(value, 25, 90);
        }

        [Editable]
        public Cell offset { get; set; }

        [Editable]
        public int transitionTime
        {
            get => _transitionTime;
            set => _transitionTime = Mathf.Max(value, 0);
        }

        [Editable]
        public Background background
        {
            get => _background;
            set
            {
                if (_background == value)
                    return;

                _background = value;

                if (isInitialLocation)
                    CameraManager.Transition(_background, 0);
            }
        }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(SignalEvent))]
        public Port signalInPort { get; set; }

        private Background _background;
        private int _zoomLevel = CameraManager.DefaultZoomLevel;
        private int _transitionTime = 4;
        private int _pitch = 55;

        [SerializeField] private bool isInitialLocation = false;

        public bool isStartingCamera => isInitialLocation;

        [ActorEventHandler]
        private void OnSignal(SignalEvent evt)
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

        public virtual void OnCameraStop()
        {
        }

        public virtual void OnCameraStart(int transitionTime)
        {
        }
    }
}
