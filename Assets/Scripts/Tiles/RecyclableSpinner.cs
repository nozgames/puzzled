using NoZ;
using UnityEngine;

namespace Puzzled
{
    abstract class RecyclableSpinner : UsableSpinner
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer[] _decalRenderers = null;

        protected abstract Decal[] decals { get; }

        protected override int stepCount => (_decalRenderers != null) ? _decalRenderers.Length : 1;
        protected override int maxValues => decals.Length;
        protected override Vector3 initialEulerAngles => new Vector3(-90, 0, -90);
        protected override Vector3 rotationAxis => new Vector3(-1, 0, 0);

        private Color _defaultColor = Color.white;
        private Vector3 _defaultScale = Vector3.one;

        private void Awake()
        {
            _defaultColor = _decalRenderers[0].color;
            _defaultScale = _decalRenderers[0].transform.localScale;
        }

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

        private void UpdateDecalRenderer(int rotateIndex, int value)
        {
            var decal = decals[value];
            var renderer = _decalRenderers[rotateIndex];
            renderer.sprite = decal.sprite;
            renderer.flipX = decal.isFlipped;
            renderer.color = decal.isAutoColor ? _defaultColor : decal.color;
            renderer.transform.localRotation = Quaternion.Euler(0, 0, decal.rotation);
            renderer.transform.localScale = _defaultScale * decal.scale;
        }

        protected void UpdateDecals()
        {
            if (decals.Length == 0)
                return;

            UpdateDecalRenderer(rotateIndex, value);
            UpdateDecalRenderer((rotateIndex + 1) % _decalRenderers.Length, WrappedValue(value + 1));
            UpdateDecalRenderer((rotateIndex + _decalRenderers.Length - 1) % _decalRenderers.Length, WrappedValue(value - 1));
       }
    }
}
