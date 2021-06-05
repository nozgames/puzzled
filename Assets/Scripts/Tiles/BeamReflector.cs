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
            public BeamDirection from;
            public BeamDirection to;

            [NonSerialized]
            public Beam beam;
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
                _rotationIndex = value % _numRotations;
                UpdateRotation();
            }
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateRotation();

        private void UpdateRotation()
        {
            float rotationStep = 360 / _numRotations;
            _rotator.localRotation = Quaternion.Euler(0, rotation * rotationStep, 0.0f);

            UpdateBeams();
        }

        [ActorEventHandler]
        private void OnBeamChangedEvent(BeamChangedEvent evt) => UpdateBeams();

        private void UpdateBeams()
        {
            foreach (var reflection in _reflections)
            {
                var beamIn = _terminal.GetBeam((BeamDirection)(((int)reflection.from + 4 + _rotationIndex) % _numRotations));

                // If there is no beam coming from the reflected direction then shut off the beam
                if (beamIn == null)
                {
                    DestroyBeam(reflection);
                    continue;
                }

                // If we already have a beam but it is for a different emitter then we need to 
                // create a new beam beause it may look different.
                if (reflection.beam == null || reflection.beam.emitter != beamIn.emitter)
                {
                    DestroyBeam(reflection);

                    if (reflection.beam != null)
                        continue;

                    beamIn = _terminal.GetBeam((BeamDirection)(((int)reflection.from + 4 + _rotationIndex) % _numRotations));
                    if (beamIn != null)
                    {
                        var reflectedDirection = (BeamDirection)(((int)reflection.to + _rotationIndex) % _numRotations);
                        reflection.beam = beamIn.emitter.InstantiateBeam(tile.transform, reflectedDirection);
                        reflection.beam.Update();
                    }
                }
            }

            _visualsOff.SetActive(!_terminal.hasBeams);
            _visualsOn.SetActive(_terminal.hasBeams);
        }

        [ActorEventHandler]
        private void OnUseEvent (UseEvent evt)
        {
            ++rotation;

            PlaySound(_useSound, 1.0f, UnityEngine.Random.Range(0.8f,1.2f));
        }

        private void DestroyBeam(Reflection reflection)
        {
            var beam = reflection.beam;
            if (null == beam)
                return;

            reflection.beam = null;
            beam.gameObject.SetActive(false);
            Destroy(beam.gameObject);
        }
    }
}
