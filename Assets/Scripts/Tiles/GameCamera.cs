using NoZ;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class GameCamera : TileComponent
    {
        [SerializeField] private GameObject _visualStartingCamera = null;

        public struct State
        {
            public Vector3 position;
            public Quaternion rotation;
            public Color bgColor;

            public State Lerp(State stateB, float t)
            {
                return new State
                {
                    position = Vector3.Lerp(position, stateB.position, t),
                    rotation = Quaternion.Slerp(rotation, stateB.rotation, t),
                    bgColor = Color.Lerp(bgColor, stateB.bgColor, t)
                };
            }
        }

        /// <summary>
        /// Non-Serialized property that sets the starting camera in the attached puzzle
        /// </summary>
        [Editable(serialized = false)]
        private bool startingCamera {
            get => puzzle.properties.startingCamera == this;
            set {
                if (value)
                {
                    if (puzzle.properties.startingCamera != null)
                        puzzle.properties.startingCamera._visualStartingCamera.gameObject.SetActive(false);

                    puzzle.properties.startingCamera = this;
                }
                else if (puzzle.properties.startingCamera == this)
                    puzzle.properties.startingCamera = null;

                _visualStartingCamera.gameObject.SetActive(value);
            }
        }

        [Editable(rangeMin = CameraManager.MinZoom, rangeMax = CameraManager.MaxZoom)]
        public int zoomLevel
        {
            get => _zoomLevel;
            set => _zoomLevel = Mathf.Clamp(value, CameraManager.MinZoom, CameraManager.MaxZoom);
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
        public Background background { get; set; }

        [Editable]
        public int priority { get; set; }

        public virtual Vector3 target => puzzle.grid.CellToWorld(tile.cell) + new Vector3(offset.x * 0.25f, 0, offset.y * 0.25f);

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(SignalEvent))]
        public Port signalInPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(OffSignal))]
        public Port offPort { get; set; }

        private int _zoomLevel = CameraManager.DefaultZoom;
        private int _transitionTime = 4;
        private int _pitch = 55;
        private float _weight = 0;
        private float _blendRate = 0;
        private bool _isActivated = false;
        public State state;

        public float weight 
        { 
            get => _weight;
            set 
            {
                _weight = Mathf.Clamp(value, 0, 1);
            } 
        }

        public bool isDead => (!_isActivated && (_weight <= 0));

        [ActorEventHandler]
        private void OnEnableEvent(SignalEvent evt)
        {
            if (isEditing)
                return;

            ActivateCamera(transitionTime);
        }

        [ActorEventHandler]
        private void OnDisableEvent(OffSignal evt)
        {
            if (isEditing)
                return;

            float totalTransitionTime = (transitionTime > 0) ? transitionTime * GameManager.tick + GameManager.tickTimeRemaining : 0;
            DeactivateCamera(totalTransitionTime);
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _visualStartingCamera.gameObject.SetActive(startingCamera);
        }

        [ActorEventHandler]
        private void OnDestroyEvent (DestroyEvent evt)
        {
            if (startingCamera)
                startingCamera = false;

            RemoveCamera();
        }

        public void ActivateCamera(int blendTime)
        {
            if (_isActivated)
                return;

            _isActivated = true;

            float totalTransitionTime = (blendTime > 0) ? blendTime * GameManager.tick + GameManager.tickTimeRemaining : 0;
            _blendRate = (totalTransitionTime > 0) ? (1 / totalTransitionTime) : float.MaxValue;

            UpdateState();
            puzzle.GetSharedComponentData<SharedCameraData>(typeof(GameCamera)).ActivateCamera(this, totalTransitionTime);
        }

        public void DeactivateCamera(float transitionTime)
        {
            _isActivated = false;
            _blendRate = weight * ((transitionTime > 0) ? -(1 / transitionTime) : -float.MaxValue);
        }
        
        public void RemoveCamera()
        {
            puzzle.GetSharedComponentData<SharedCameraData>(typeof(GameCamera)).RemoveCamera(this);
        }

        public void UpdateState()
        {
            state.position = CameraManager.Frame(target, pitch, zoomLevel, CameraManager.FieldOfView);
            state.rotation = Quaternion.Euler(pitch, 0, 0);
            state.bgColor = background?.color ?? CameraManager.defaultBackground.color;
        }

        public void BlendUpdate()
        {
            weight += _blendRate * Time.deltaTime;
            UpdateState();
        }
    }
}
