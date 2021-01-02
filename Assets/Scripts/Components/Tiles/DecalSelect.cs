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
        [Port(PortFlow.Input, PortType.Number)]
        public Port selectPort { get; set; }

        /// <summary>
        /// Output used to send the current selected decal index
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        public Port valuePort { get; set; }

        private void UpdateDecal(int decalIndex)
        {
            if (isEditing || isLoading || decals == null)
                return;

            _decalIndex = Mathf.Clamp(decalIndex, 1, decals.Length);

            valuePort.SendValue(_decalIndex);

            var surface = DecalSurface.FromCell(puzzle, tile.cell);
            if (surface != null)
                surface.decal = decals[_decalIndex - 1];
        }

        [ActorEventHandler]
        private void OnSignalEvent(ValueSignalEvent evt) => UpdateDecal(evt.value);
    }
}
