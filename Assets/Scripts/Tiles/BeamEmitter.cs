using UnityEngine;
using NoZ;

namespace Puzzled
{
    [RequireComponent(typeof(Usable))]
    public class BeamEmitter : TileComponent
    {
        [SerializeField] private GameObject _visualsOn = null;
        [SerializeField] private GameObject _visualsOff = null;
        [SerializeField] private AudioClip _useSound = null;
        [SerializeField] private Transform _rotator = null;

        [Tooltip("Prefab to use to create a new beam")]
        [SerializeField] private GameObject _beamPrefab = null;

        [SerializeField]
        private int _numRotations = 8;

        private bool _isEmitting = false;

        private int _rotationIndex = 0;

        private Beam _beam = null;

        [Editable(hidden = true)]
        public int rotation
        {
            get => _rotationIndex;
            set
            {
                _rotationIndex = value % _numRotations;
                UpdateBeam();
            }
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateBeam();

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
            _isEmitting = !puzzle.isPreview && _isEmitting;

            if (null != _rotator)
                _rotator.localRotation = Beam.GetRotation((BeamDirection)rotation);

            if (!_isEmitting)
            {
                if(_beam != null)
                {
                    _beam.gameObject.SetActive(false);
                    Destroy(_beam.gameObject);
                    _beam = null;
                }                
                return;
            }

            if (_beam == null)
                _beam = InstantiateBeam(tile.transform, (BeamDirection)rotation);
            else
                _beam.direction = (BeamDirection)rotation;

            _beam.Update();
        }

        public Beam InstantiateBeam (Transform parent, BeamDirection direction)
        {
            var beam = Instantiate(_beamPrefab, parent).GetComponent<Beam>();
            beam.emitter = this;
            beam.name = parent.name + "_Beam_" + direction;
            beam.direction = direction;
            return beam;
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

