using NoZ;
using UnityEngine;
using System.Linq;

namespace Puzzled
{
    class DecalSpinner : RecyclableSpinner
    {
        /// <summary>
        /// List of all decals
        /// </summary>
        [Editable]
        public Decal[] spinnerDecals { get; private set; }

        override protected Decal[] decals => spinnerDecals;

        protected override void OnStart(StartEvent evt)
        {
            if (spinnerDecals == null)
                spinnerDecals = new Decal[0];

            base.OnStart(evt);
        }
    }
}
