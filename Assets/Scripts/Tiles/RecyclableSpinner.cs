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
            _decalRenderers[rotateIndex].sprite = decal.sprite;
            _decalRenderers[rotateIndex].flipX = decal.isFlipped;
            _decalRenderers[rotateIndex].transform.transform.localRotation = Quaternion.Euler(0, 0, decal.rotation);
        }

        private void UpdateDecals()
        {
            if (decals.Length == 0)
                return;

            UpdateDecalRenderer(rotateIndex, value);
            UpdateDecalRenderer((rotateIndex + 1) % _decalRenderers.Length, WrappedValue(value + 1));
            UpdateDecalRenderer((rotateIndex + _decalRenderers.Length - 1) % _decalRenderers.Length, WrappedValue(value - 1));
       }
    }
}
