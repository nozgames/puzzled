using UnityEngine;
using NoZ;

namespace Puzzled
{
    class BlockRaycast : TileComponent
    {
        [SerializeField] private float _offset = 0.0f;

        [ActorEventHandler]
        private void OnRayCastEvent (RayCastEvent evt)
        {
            evt.hit = tile;
            evt.offset = _offset;
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

        [ActorEventHandler]
        private void OnDestroyEvent (DestroyEvent evt)
        {
            enabled = false;
        }

        [ActorEventHandler]
        private void OnCellChangedEvent(CellChangedEvent evt)
        {
            Beam.Refresh(puzzle, tile.cell);
        }
    }
}
