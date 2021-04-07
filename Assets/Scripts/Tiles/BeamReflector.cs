using System;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    [RequireComponent(typeof(Usable))]
    public class BeamReflector : TileComponent
    {
        [Serializable]
        private class Reflection
        {
            public Beam beam;
            public BeamDirection from;
            public BeamDirection to;
        }

        [SerializeField] private Reflection[] _reflections = null;
        [SerializeField] private BeamTerminal _terminal = null;
        [SerializeField] private Transform _rotator = null;
        [SerializeField] private GameObject _visualsOn = null;
        [SerializeField] private GameObject _visualsOff = null;
        [SerializeField] private AudioClip _useSound = null;

        private int _rotationIndex = 3;

        [SerializeField]
        private int _numRotations = 8;

        [Editable(hidden = true)]
        public int rotation
        {
            get => _rotationIndex;
            set
            {
                this._rotationIndex = value % _numRotations;
                UpdateRotation();
            }
        }

        [ActorEventHandler]
        private void OnAwakeEvent(BeamChangedEvent evt)
        {
            foreach (var reflection in _reflections)
            {
                reflection.beam.direction = reflection.to;
                reflection.beam.length = 20;
            }
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateRotation();
        }

        private void UpdateRotation()
        {
            float rotationStep = 360 / _numRotations;
            _rotator.localRotation = Quaternion.Euler(0, rotation * rotationStep, 0.0f);

            UpdateBeams();
        }

        [ActorEventHandler]
        private void OnBeamChangedEvent(BeamChangedEvent evt) => UpdateBeams();

        private BeamDirection GetReflectedBeam(BeamDirection inDirection)
        {
            int directionDistance = Mathf.Abs((int)inDirection - rotation);

            BeamDirection reflectedDirection = inDirection;
            if ((directionDistance % 2) == 1)
            {
                BeamDirection simplifiedRotation = (BeamDirection)(rotation % 4);
                switch (inDirection)
                {
                    case BeamDirection.SouthEast:
                        {
                            switch (simplifiedRotation)
                            {
                                case BeamDirection.North:
                                    reflectedDirection = BeamDirection.NorthEast;
                                    break;
                                case BeamDirection.East:
                                    reflectedDirection = BeamDirection.SouthWest;
                                    break;
                            }
                        }
                        break;
                    case BeamDirection.South:
                        {
                            switch (simplifiedRotation)
                            {
                                case BeamDirection.SouthEast:
                                    reflectedDirection = BeamDirection.West;
                                    break;
                                case BeamDirection.NorthEast:
                                    reflectedDirection = BeamDirection.East;
                                    break;
                            }
                        }
                        break;
                    case BeamDirection.SouthWest:
                        {
                            switch (simplifiedRotation)
                            {
                                case BeamDirection.North:
                                    reflectedDirection = BeamDirection.NorthWest;
                                    break;
                                case BeamDirection.East:
                                    reflectedDirection = BeamDirection.SouthEast;
                                    break;
                            }
                        }
                        break;
                    case BeamDirection.West:
                        {
                            switch (simplifiedRotation)
                            {
                                case BeamDirection.NorthEast:
                                    reflectedDirection = BeamDirection.North;
                                    break;
                                case BeamDirection.SouthEast:
                                    reflectedDirection = BeamDirection.South;
                                    break;
                            }
                        }
                        break;
                    case BeamDirection.NorthWest:
                        {
                            switch (simplifiedRotation)
                            {
                                case BeamDirection.North:
                                    reflectedDirection = BeamDirection.West;
                                    break;
                                case BeamDirection.East:
                                    reflectedDirection = BeamDirection.South;
                                    break;
                            }
                        }
                        break;
                    case BeamDirection.North:
                        {
                            switch (simplifiedRotation)
                            {
                                case BeamDirection.NorthEast:
                                    reflectedDirection = BeamDirection.West;
                                    break;
                                case BeamDirection.SouthEast:
                                    reflectedDirection = BeamDirection.East;
                                    break;
                            }
                        }
                        break;
                    case BeamDirection.NorthEast:
                        {
                            switch (simplifiedRotation)
                            {
                                case BeamDirection.North:
                                    reflectedDirection = BeamDirection.SouthEast;
                                    break;
                                case BeamDirection.East:
                                    reflectedDirection = BeamDirection.NorthWest;
                                    break;
                            }
                        }
                        break;
                    case BeamDirection.East:
                        {
                            switch (simplifiedRotation)
                            {
                                case BeamDirection.NorthEast:
                                    reflectedDirection = BeamDirection.South;
                                    break;
                                case BeamDirection.SouthEast:
                                    reflectedDirection = BeamDirection.North;
                                    break;
                            }
                        }
                        break;
                }
            }

            return reflectedDirection;
        }

        private void UpdateBeams()
        {
            var hasReflection = false;

            foreach (var reflection in _reflections)
            {
                bool isPowered = false;
                for (int i = _terminal.connectionCount - 1; i >= 0; i--)
                {
                    BeamDirection reflectedDirection = GetReflectedBeam(_terminal.GetConnection(i).direction);

                    if (reflectedDirection == _terminal.GetConnection(i).direction)
                        break;

                    if (reflection.to == reflectedDirection)
                    {
                        isPowered = true;
                        hasReflection = true;
                        break;
                    }
                }

                reflection.beam.gameObject.SetActive(isPowered);
            }

            _visualsOff.SetActive(!hasReflection);
            _visualsOn.SetActive(hasReflection);
        }

        [ActorEventHandler]
        private void OnUseEvent (UseEvent evt)
        {
            ++rotation;

            PlaySound(_useSound, 1.0f, UnityEngine.Random.Range(0.8f,1.2f));
        }
    }
}
