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

        private bool _rotated = false;

        [Editable(hidden = true)]
        public bool rotated {
            get => _rotated;
            set {
                if (_rotated == value)
                    return;
                _rotated = value;
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
            _rotator.localRotation = Quaternion.Euler(0, _rotated ? 90.0f : 0.0f, 0.0f);

            UpdateBeams();
        }

        [ActorEventHandler]
        private void OnBeamChangedEvent(BeamChangedEvent evt) => UpdateBeams();

        private void UpdateBeams()
        {
            var hasReflection = false;

            foreach(var reflection in _reflections)
            {
                var from = (BeamDirection)(((int)reflection.from + (_rotated ? 4 : 0)) % 8);
                var powered = false;
                for(int i=_terminal.connectionCount - 1; i>=0 && !powered; i--)
                    powered = _terminal.GetConnection(i).direction == from;

                reflection.beam.gameObject.SetActive(powered);
                hasReflection |= powered;
            }

            _visualsOff.SetActive(!hasReflection);
            _visualsOn.SetActive(hasReflection);
        }

        [ActorEventHandler]
        private void OnUseEvent (UseEvent evt)
        {
            rotated = !rotated;

            PlaySound(_useSound, 1.0f, UnityEngine.Random.Range(0.8f,1.2f));
        }
    }
}
