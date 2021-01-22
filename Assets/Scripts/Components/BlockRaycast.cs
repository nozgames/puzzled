using UnityEngine;
using NoZ;

namespace Puzzled
{
    class BlockRaycast : TileComponent
    {
        [ActorEventHandler]
        private void OnRayCastEvent (RayCastEvent evt)
        {
            evt.hit = tile;
        }

        [ActorEventHandler]
        private void OnStartEvent (StartEvent evt)
        {
            Beam.Refresh(puzzle, tile.cell);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if(puzzle != null)
                Beam.Refresh(puzzle, tile.cell);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Beam.Refresh(puzzle, tile.cell);
        }
    }
}
