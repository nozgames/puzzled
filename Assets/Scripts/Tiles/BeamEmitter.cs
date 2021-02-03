using UnityEngine;
using NoZ;

namespace Puzzled
{
    class BeamEmitter : UsableTileComponent
    {
        [SerializeField] private GameObject _visualsOn = null;
        [SerializeField] private GameObject _visualsOff = null;
        [SerializeField] private AudioClip _useSound = null;
        [SerializeField] private Beam _beam = null;
        [SerializeField] private Transform _rotator = null;

        private int _value = 0;

        private static readonly BeamDirection[] _dirs = { 
            BeamDirection.North,
            BeamDirection.NorthEast,
            BeamDirection.East,
            BeamDirection.SouthEast,
            BeamDirection.South,
            BeamDirection.SouthWest,
            BeamDirection.West,
            BeamDirection.NorthWest
        };

        [Editable]
        private int value {
            get => _value;
            set {
                _value = (value % _dirs.Length);
                UpdateBeam();
            }
        }

        public bool isEmitting => isUsable;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _beam.isPowered = true;
            UpdateBeam();
        }

        [ActorEventHandler]
        private void OnPreviewStartEvent (PreviewStartEvent evt)
        {
            _beam.gameObject.SetActive(false);
        }

        [ActorEventHandler]
        private void OnValueEvent(ValueEvent evt)
        {
            value = evt.value - 1;
            UpdateBeam();
        }

        [ActorEventHandler]
        private void OnUseEvent(UseEvent evt)
        {
            if (!isUsable)
                return;

            evt.IsHandled = true;
            value++;
            PlaySound(_useSound);
        }

        [ActorEventHandler]
        private void OnBeamChangedEvent (BeamChangedEvent evt) => OnUsableChanged();

        private void UpdateBeam()
        {
            if (null != _rotator)
                _rotator.localRotation = Beam.GetRotation((BeamDirection)value);

            if (!isEmitting)
            {
                _beam.gameObject.SetActive(false);
                return;
            }

            _beam.gameObject.SetActive(true);
            _beam.direction = _dirs[value];
            _beam.length = 10;
        }

        protected override void OnUsableChanged()
        {
            base.OnUsableChanged();

            _visualsOff.gameObject.SetActive(!isEmitting);
            _visualsOn.gameObject.SetActive(isEmitting);
            UpdateBeam();
        }
    }
}

