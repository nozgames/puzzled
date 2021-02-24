using NoZ;
using UnityEngine;

namespace Puzzled
{
    abstract class UsableSpinner : UsableTileComponent
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer[] _decalRenderers = null;
        [SerializeField] private Transform _rotator = null;
        [SerializeField] private AudioClip _useSound = null;

        abstract protected Sprite[] sprites { get; }

        private int _target = 0;
        private int _value = 0;
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
        public int target
        {
            get => _target;
            set
            {
                _target = value;
                OnUpdateValue();
            }
        }

        [Editable]
        public int value
        {
            get => _value;
            set
            {
                if (value == _value)
                    return;

                var old = _value;

                if (sprites == null || sprites.Length == 0)
                    _value = 0;
                else
                    _value = ClampedValue(value);

                UpdateDecals();
                OnUpdateValue();
            }
        }

        virtual protected void InitializeSprites()
        {
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            InitializeSprites();
            _rotator.localRotation = Quaternion.Euler(-90 - _rotateIndex * 60.0f, 0, -90);
            UpdateDecals();
            OnUpdateValue();
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;

            if (!isUsable)
                return;

            var oldIndex = _rotateIndex;
            _rotateIndex = (_rotateIndex + 1) % _decalRenderers.Length;

            PlaySound(_useSound, 1, 1.4f);

            var step = (360.0f / _decalRenderers.Length);
            GameManager.busy++;
            Tween.Rotate(
                new Vector3(-90 - oldIndex * step, 0, -90),
                new Vector3(-90 - (oldIndex + 1) * step, 0, -90))
                .Duration(0.25f)
                .OnStop(OnUseComplete)
                .Start(_rotator.gameObject);
        }

        private void OnUseComplete()
        {
            value++;
            UpdateDecals();
            GameManager.busy--;
        }

        [ActorEventHandler]
        private void OnIncrement(IncrementSignal evt) => value++;

        private void UpdateDecals()
        {
            if (sprites.Length == 0)
                return;

            _decalRenderers[_rotateIndex].sprite = sprites[value];
            _decalRenderers[(_rotateIndex + 1) % _decalRenderers.Length].sprite = sprites[WrappedValue(value + 1)];
            _decalRenderers[(_rotateIndex + _decalRenderers.Length - 1) % _decalRenderers.Length].sprite = sprites[WrappedValue(value - 1)];
        }

        private int ClampedValue(int value) => Mathf.Clamp(1 + (value % sprites.Length), 1, sprites.Length);
        private int WrappedValue(int value) => ((value + sprites.Length - 1) % sprites.Length);

        private void OnUpdateValue()
        {
            if (tile == null || isLoading)
                return;

            valueOutPort.SendValue(value, true);
            powerOutPort.SetPowered(value == _target);
        }
    }
}
