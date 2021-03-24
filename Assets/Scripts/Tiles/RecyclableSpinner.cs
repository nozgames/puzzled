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
            _decalRenderers[rotateIndex].sprite = decals[value].sprite;
            _decalRenderers[rotateIndex].flipX = (decals[value].flags & DecalFlags.FlipHorizontal) == DecalFlags.FlipHorizontal;
            _decalRenderers[rotateIndex].flipY = (decals[value].flags & DecalFlags.FlipVertical) == DecalFlags.FlipVertical;
            _decalRenderers[rotateIndex].transform.transform.localRotation = Quaternion.Euler(0, 0, ((decals[value].flags & DecalFlags.Rotate) == DecalFlags.Rotate) ? -90 : 0);
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
