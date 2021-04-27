using NoZ;
using UnityEngine;
using System.Linq;

namespace Puzzled
{
    class DecalSpinner : RecyclableSpinner
    {
        private Decal[] _decals;

        /// <summary>
        /// List of all decals
        /// </summary>
        [Editable]
        public Decal[] spinnerDecals {
            get => _decals;
            set {
                _decals = value;

                if(isEditing)
                    UpdateDecals();
            }
        }

        override protected Decal[] decals => spinnerDecals;

        protected override void OnStart(StartEvent evt)
        {
            if (spinnerDecals == null)
                spinnerDecals = new Decal[0];

            base.OnStart(evt);
        }
    }
}
