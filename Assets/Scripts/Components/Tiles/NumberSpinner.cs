using NoZ;
using UnityEngine;

namespace Puzzled
{
    class NumberSpinner : TileComponent
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer[] _decalRenderers = null;
        [SerializeField] private Sprite[] _numberDecals = null;
        [SerializeField] private Transform _rotator = null;
        [SerializeField] private AudioClip _useSound = null;

        private int _target = 1;
        private int _value = 1;
        private int _rotateIndex = 0;

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        public Port valueOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(IncrementSignal))]
        public Port incrementPort { get; set; }

        [Editable]
        public int target {
            get => _target;
            set {
                _target = value;
                OnUpdateValue();
            }
        }

        [Editable]
        public int value {
            get => _value;
            set {
                if (value == _value)
                    return;

                var old = _value;

                if (_numberDecals == null || _numberDecals.Length == 0)
                    _value = 1;
                else
                    _value = ClampedValue(value);

                UpdateDecals();
                OnUpdateValue();
            }
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _rotator.localRotation = Quaternion.Euler(-90 - _rotateIndex * 60.0f, 0, -90);
            UpdateDecals();
            OnUpdateValue();
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;

            var oldIndex = _rotateIndex;
            _rotateIndex = (_rotateIndex + 1) % _decalRenderers.Length;

            PlaySound(_useSound, 1, 1.4f);

            var step = (360.0f / _decalRenderers.Length);
            GameManager.busy++;
            Tween.Rotate(
                new Vector3(-90 - oldIndex * step, 0, -90),
                new Vector3(-90 - (oldIndex+1) * step, 0, -90))
                .Duration(0.25f)
                .OnStop(OnUseComplete)
                .Start(_rotator.gameObject);
        }

        private void OnUseComplete ()
        {
            value++;
            UpdateDecals();
            GameManager.busy--;
        }

        [ActorEventHandler]
        private void OnIncrement(IncrementSignal evt) => value++;

        private void UpdateDecals ()
        {
            _decalRenderers[_rotateIndex].sprite = _numberDecals[value - 1];
            _decalRenderers[(_rotateIndex + 1) % _decalRenderers.Length].sprite = _numberDecals[WrappedValue(value + 1) - 1];
            _decalRenderers[(_rotateIndex + _decalRenderers.Length - 1) % _decalRenderers.Length].sprite = _numberDecals[WrappedValue(value - 1) - 1];
        }

        private int ClampedValue (int value) => Mathf.Clamp(1 + ((value - 1) % _numberDecals.Length), 1, _numberDecals.Length);
        private int WrappedValue (int value) => ((value + _numberDecals.Length - 1) % _numberDecals.Length) + 1;

        private void OnUpdateValue()
        {
            if (tile == null)
                return;

            valueOutPort.SendValue(value, true);
            powerOutPort.SetPowered(value == _target);
        }
    }
}
