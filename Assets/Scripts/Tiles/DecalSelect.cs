using UnityEngine;
using NoZ;

namespace Puzzled
{
    [RequireComponent(typeof(Select))]
    class DecalSelect : TileComponent
    {
        /// <summary>
        /// List of all decals
        /// </summary>
        [Editable]
        public Decal[] decals { get; private set; }

        [ActorEventHandler]
        private void OnSelectUpdate(SelectUpdateEvent evt)
        {
            if (isEditing || isLoading || decals == null)
                return;

            // get first 

            if (evt.transientValue <= 0)
                return;

            var surface = DecalSurface.FromCell(puzzle, tile.cell);
            if (surface != null)
                surface.decal = decals[evt.transientValue];
        }
    }
}
