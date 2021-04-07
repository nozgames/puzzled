using UnityEngine;
using NoZ;

namespace Puzzled
{
    [RequireComponent(typeof(Usable))]
    class BeamEmitter : TileComponent
    {
        [SerializeField] private GameObject _visualsOn = null;
        [SerializeField] private GameObject _visualsOff = null;
        [SerializeField] private AudioClip _useSound = null;
        [SerializeField] private Beam _beam = null;
        [SerializeField] private Transform _rotator = null;

        private bool _isEmitting = false;

        private int _rotationIndex = 0;

        [SerializeField]
        private int _numRotations = 8;

        [Editable(hidden = true)]
        public int rotation
        {
            get => _rotationIndex;
            set
            {
                this._rotationIndex = value % _numRotations;
                UpdateBeam();
            }
        }

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
            rotation = evt.value;
            UpdateBeam();
        }

        [ActorEventHandler]
        private void OnUseEvent(UseEvent evt)
        {
            evt.IsHandled = true;
            rotation++;
            PlaySound(_useSound);
        }

        private void UpdateBeam()
        {
            if (null != _rotator)
                _rotator.localRotation = Beam.GetRotation((BeamDirection)rotation);

            if (!_isEmitting)
            {
                _beam.gameObject.SetActive(false);
                return;
            }

            _beam.gameObject.SetActive(true);
            _beam.direction = (BeamDirection)rotation;
            _beam.length = 10;
        }

        [ActorEventHandler]
        private void OnUsableChanged(UsableChangedEvent evt)
        {
            _isEmitting = evt.isUsable;
            _visualsOff.gameObject.SetActive(!_isEmitting);
            _visualsOn.gameObject.SetActive(_isEmitting);
            UpdateBeam();
        }
    }
}

