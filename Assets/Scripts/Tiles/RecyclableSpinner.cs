using NoZ;
using UnityEngine;

namespace Puzzled
{
    abstract class RecyclableSpinner : UsableSpinner
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer[] _decalRenderers = null;

        protected abstract Sprite[] sprites { get; }
        protected override int stepCount => (_decalRenderers != null) ? _decalRenderers.Length : 1;
        protected override int maxValues => sprites.Length;
        protected override Vector3 initialEulerAngles => new Vector3(-90, 0, -90);
        protected override Vector3 rotationAxis => new Vector3(-1, 0, 0);

        protected override void OnUseComplete()
        {
            base.OnUseComplete();
            UpdateDecals();
        }

        override protected void OnPortValuesUpdated()
        {
            base.OnPortValuesUpdated();
            UpdateDecals();
        }

        protected override void OnStart(StartEvent evt)
        {
            UpdateDecals();
            base.OnStart(evt);
        }

        private void UpdateDecals()
        {
            if (sprites.Length == 0)
                return;

            _decalRenderers[rotateIndex].sprite = sprites[value];
            _decalRenderers[(rotateIndex + 1) % _decalRenderers.Length].sprite = sprites[WrappedValue(value + 1)];
            _decalRenderers[(rotateIndex + _decalRenderers.Length - 1) % _decalRenderers.Length].sprite = sprites[WrappedValue(value - 1)];
        }
    }
}
