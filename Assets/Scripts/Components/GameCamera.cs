﻿using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class GameCamera : TileComponent
    {
        [SerializeField] private GameObject _visualStartingCamera = null;

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

        public virtual Vector3 target => puzzle.grid.CellToWorld(tile.cell) + new Vector3(offset.x * 0.25f, 0, offset.y * 0.25f);

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(SignalEvent))]
        public Port signalInPort { get; set; }

        private int _zoomLevel = CameraManager.DefaultZoom;
        private int _transitionTime = 4;
        private int _pitch = 55;

        [ActorEventHandler]
        private void OnSignal(SignalEvent evt)
        {
            if (isEditing)
                return;

            puzzle.SetActiveCamera(this);
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
        }

        public virtual void OnCameraStop()
        {
        }

        public virtual void OnCameraStart(int transitionTime)
        {
        }
    }
}
