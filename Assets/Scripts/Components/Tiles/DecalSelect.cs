using UnityEngine;
using NoZ;

namespace Puzzled
{
    class DecalSelect : TileComponent
    {
        private int _decalIndex;

        /// <summary>
        /// List of all decals
        /// </summary>
        [Editable]
        public Decal[] decals { get; private set; }

        /// <summary>
        /// Selected decal
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Number, legacy = true)]
        private Port valueInPort { get; set; }

        /// <summary>
        /// Output used to send the current selected decal index
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        private Port valueOutPort { get; set; }

        private void UpdateDecal(int decalIndex)
        {
            if (isEditing || isLoading || decals == null)
                return;

            _decalIndex = Mathf.Clamp(decalIndex, 1, decals.Length);

            valueOutPort.SendValue(_decalIndex);

            var surface = DecalSurface.FromCell(puzzle, tile.cell);
            if (surface != null)
                surface.decal = decals[_decalIndex - 1];
        }

        [ActorEventHandler]
        private void OnSignalEvent(ValueEvent evt) => UpdateDecal(evt.value);
    }
}
