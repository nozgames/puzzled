using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Usable))]
    abstract class UsableSpinner : Spinner
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer[] _decalRenderers = null;
        [SerializeField] private Transform _rotator = null;
        [SerializeField] private AudioClip _useSound = null;

        abstract protected Sprite[] sprites { get; }

        private int _rotateIndex = 0;

        override protected int maxValues => sprites.Length;

        virtual protected void InitializeSprites()
        {
        }

        protected override void OnStart(StartEvent evt)
        {
            InitializeSprites();
            _rotator.localRotation = Quaternion.Euler(-90 - _rotateIndex * 60.0f, 0, -90);

            base.OnStart(evt);
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

        override protected void OnPortValuesUpdated()
        {
            base.OnPortValuesUpdated();
            UpdateDecals();
        }

        private void UpdateDecals()
        {
            if (sprites.Length == 0)
                return;

            _decalRenderers[_rotateIndex].sprite = sprites[value];
            _decalRenderers[(_rotateIndex + 1) % _decalRenderers.Length].sprite = sprites[WrappedValue(value + 1)];
            _decalRenderers[(_rotateIndex + _decalRenderers.Length - 1) % _decalRenderers.Length].sprite = sprites[WrappedValue(value - 1)];
        }
    }
}
