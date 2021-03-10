using NoZ;
using UnityEngine;
using System.Linq;

namespace Puzzled
{
    class DecalSpinner : UsableSpinner
    {
        private Sprite[] _decalSprites = null;

        /// <summary>
        /// List of all decals
        /// </summary>
        [Editable]
        public Decal[] decals { get; private set; }

        override protected Sprite[] sprites => _decalSprites;

        protected override void OnStart(StartEvent evt)
        {
            _decalSprites = decals?.Select(d => d.sprite).ToArray() ?? new Sprite[0];
            base.OnStart(evt);
        }
    }
}
