using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Cycle))]
    public class DecalCycle : TileComponent
    {
        private int _decalIndex;

        /// <summary>
        /// List of all decals
        /// </summary>
        [Editable]
        public Decal[] decals { get; private set; }

        /// <summary>
        /// Output port used to send the current cycle value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        private Port valueOutPort { get; set; }

        [ActorEventHandler]
        private void OnCycleAdvance(CycleAdvanceEvent evt)
        {
            ++_decalIndex;

            if (_decalIndex >= decals.Length)
            {
                if (evt.isLooping)
                    _decalIndex = 0;
                else
                    _decalIndex = decals.Length - 1;
            }
        }

        [ActorEventHandler]
        private void OnCycleUpdate(CycleUpdateEvent evt)
        {
            UpdateDecal();
        }

        [ActorEventHandler]
        private void OnCycleReset(CycleResetEvent evt)
        {
            _decalIndex = 0;
        }

        private void UpdateDecal()
        {
            if (isEditing || isLoading || decals == null)
                return;
            
            valueOutPort.SendValue(_decalIndex);

            var surface = DecalSurface.FromCell(puzzle, tile.cell);
            if (surface != null)
                surface.decal = decals[_decalIndex];
        }
    }
}
