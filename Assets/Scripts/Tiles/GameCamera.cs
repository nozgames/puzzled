using NoZ;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public partial class GameCamera : TileComponent
    {
        [SerializeField] private GameObject _visualStartingCamera = null;

        public struct State
        {
            public Vector3 targetPosition;
            public float pitch;
            public float yaw;
            public int yawIndex;
            public float zoomLevel;
            public Color bgColor;
            public bool isBusy;

            public void Lerp(State stateB, float t)
            {
                targetPosition = Vector3.Lerp(targetPosition, stateB.targetPosition, t);
                pitch = Mathf.LerpAngle(pitch, stateB.pitch, t);
                yaw = Mathf.LerpAngle(yaw, stateB.yaw, t);
                yawIndex = (t >= 0.5) ? stateB.yawIndex : yawIndex;
                zoomLevel = Mathf.Lerp(zoomLevel, stateB.zoomLevel, t);
                bgColor = Color.Lerp(bgColor, stateB.bgColor, t);
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

        public int yaw
        {
            get => _yawIndex * 90;
        }

        [Editable(rangeMin = 0, rangeMax = 3)]
        public int yawIndex
        {
            get => _yawIndex;
            set => _yawIndex = Mathf.Clamp(value, 0, 3);
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
        public int layer { get; set; }

        [Editable]
        public bool busyDuringTransition { get; set; } = true;

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
        private int _yawIndex = 0;
        private float _weight = 0;
        private float _blendRate = 0;
        private bool _isActivated = false;

        private SharedCameraData _sharedCamDataInternalOnly = null;
        public State state;

        private SharedCameraData sharedCamData
        {
            get
            { 
                if (_sharedCamDataInternalOnly == null)
                    _sharedCamDataInternalOnly = puzzle.GetSharedComponentData<SharedCameraData>(typeof(GameCamera));

                return _sharedCamDataInternalOnly;
            }
        }

        public float weight 
        { 
            get => _weight;
            set 
            {
                _weight = Mathf.Clamp(value, 0, 1);
                if (!isBlending)
                    state.isBusy = false; // clear busy if not blending anymore
            } 
        }

        public bool isDead => (!_isActivated && (_weight <= 0));
        public bool isBlending => (_isActivated ? (_weight < 1) : (_weight > 0));
        public float remainingTransitionTime => (_isActivated ? (1 - weight) : (weight) / _blendRate);

        [ActorEventHandler]
        private void OnEnableEvent(SignalEvent evt)
        {
            if (isEditing)
                return;

            float totalTransitionTime = (transitionTime > 0) ? ((1 - weight) * (transitionTime * GameManager.tick + GameManager.tickTimeRemaining)) : 0;
            ActivateCamera(totalTransitionTime);
        }

        [ActorEventHandler]
        private void OnDisableEvent(OffSignal evt)
        {
            if (isEditing)
                return;

            float totalTransitionTime = (transitionTime > 0) ? (weight * (transitionTime * GameManager.tick + GameManager.tickTimeRemaining)) : 0;
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

        public static void Initialize(Puzzle puzzle)
        {
            SharedCameraData sharedCamData = new SharedCameraData();
            puzzle.SetSharedComponentData(typeof(GameCamera), sharedCamData);

            if (!puzzle.isEditing)
            {
                if (puzzle.player != null)
                {
                    sharedCamData.baseCameraState = new State
                    {
                        targetPosition = puzzle.grid.CellToWorld(puzzle.player.tile.cell),
                        yaw = CameraManager.DefaultYaw,
                        pitch = CameraManager.DefaultPitch,
                        zoomLevel = CameraManager.DefaultZoom,
                        bgColor = CameraManager.defaultBackground.color
                    };
                }
            }
        }

        public static State UpdateCameraBlendingState()
        {
            SharedCameraData cameraData = GameManager.puzzle.GetSharedComponentData<SharedCameraData>(typeof(GameCamera));
            return cameraData.UpdateCameraBlendingState();
        }

        public void ActivateCamera(float blendTime)
        {
            if (_isActivated)
                return;

            _isActivated = true;

            UpdateState();
            sharedCamData.ActivateCamera(this, blendTime);
        }

        private void SimpleDeactivateCamera(float transitionTime)
        {
            _isActivated = false;
            if (transitionTime > 0)
                SetBlendOutTime(transitionTime);
            else
                SnapToTargetWeight();
        }

        public void DeactivateCamera(float blendTime)
        {
            if (!_isActivated)
                return;

            _isActivated = false;
            sharedCamData.DeactivateCamera(this, blendTime);
        }

        public void SetBlendInTime(float transitionTime)
        {
            _blendRate = (1 - weight) * ((transitionTime > 0) ? (1 / transitionTime) : float.MaxValue);
        }

        public void SetBlendOutTime(float transitionTime)
        {
            _blendRate = weight * ((transitionTime > 0) ? -(1 / transitionTime) : -float.MaxValue);
        }

        public void UpdateBlendRate(float transitionTime)
        {
            if (_blendRate > 0)
                SetBlendInTime(transitionTime);
            else
                SetBlendOutTime(transitionTime);
        }

        public void RemoveCamera()
        {
            sharedCamData.RemoveCamera(this);
        }

        public void UpdateState()
        {
            state.targetPosition = target;
            state.pitch = pitch;
            state.yaw = yaw;
            state.yawIndex = _yawIndex;
            state.zoomLevel = zoomLevel;
            state.bgColor = background?.color ?? CameraManager.defaultBackground.color;
        }

        public void SnapToTargetWeight()
        {
            weight = _isActivated ? 1 : 0; // jump to target weight if not expressed
            _blendRate = 0;
        }

        public void BlendUpdate()
        {
            weight += _blendRate * Time.deltaTime;

            UpdateState();
        }
    }
}
