using UnityEngine;
using NoZ;

namespace Puzzled
{
    class DecalSelect : TileComponent
    {
        [Editable]
        public Decal[] decals { get; private set; }

        private void UpdateDecal(int decalIndex)
        {
            if (isEditing || isLoading || decals == null)
                return;

            decalIndex = Mathf.Clamp(decalIndex, 0, decals.Length - 1);

            var surface = DecalSurface.FromCell(puzzle, tile.cell);
            if (surface != null)
                surface.decal = decals[decalIndex];
        }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt) => UpdateDecal(evt.wire.value - 1);

        [ActorEventHandler]
        private void OnWireValueChanged(WireValueChangedEvent evt) => UpdateDecal(evt.wire.value - 1);
    }
}
