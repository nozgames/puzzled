using NoZ;

namespace Puzzled
{
    public class TileComponent : ActorComponent
    {
        public Tile tile => (Tile)base.actor;

        public bool SendToCell(ActorEvent evt, Cell cell, CellEventRouting routing = CellEventRouting.All) =>
            TileGrid.SendToCell(evt, cell, routing);

        protected void BeginBusy() => GameManager.busy++;

        protected void EndBusy() => GameManager.busy--;
    }
}
