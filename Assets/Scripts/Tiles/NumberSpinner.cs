using NoZ;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    class NumberSpinner : RecyclableSpinner
    {
        private Decal[] _decals = null;

        [Header("Visuals")]
        [SerializeField] private Texture[] _numberTextures = null;

        override protected Decal[] decals => _decals;

        protected override void OnStart(StartEvent evt)
        {
            _decals = _numberTextures?.Select(s => new Decal(System.Guid.Empty, s)).ToArray() ?? new Decal[0];
            base.OnStart(evt);
        }
    }
}
