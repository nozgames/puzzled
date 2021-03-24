using NoZ;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    class NumberSpinner : RecyclableSpinner
    {
        private Decal[] _spriteDecals = null;

        [Header("Visuals")]
        [SerializeField] private Sprite[] _numberSprites = null;

        override protected Decal[] decals => _spriteDecals;

        protected override void OnStart(StartEvent evt)
        {
            _spriteDecals = _numberSprites?.Select(s => new Decal(System.Guid.Empty, s)).ToArray() ?? new Decal[0];
            base.OnStart(evt);
        }
    }
}
