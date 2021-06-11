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
            if (isLoading || decals == null)
                return;

            if (evt.transientValue < 0 || evt.transientValue >= decals.Length)
                return;

            var surfaces = DecalSurface.FromCell(puzzle, tile.cell);
            if (surfaces != null)
                foreach(var surface in surfaces)
                    surface.decal = decals[evt.transientValue];
        }
    }
}
