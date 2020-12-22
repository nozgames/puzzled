using NoZ;

namespace Puzzled
{
    class CellChangedEvent : ActorEvent
    {
        public Cell old { get; private set; }
        public Tile tile { get; private set; }

        public CellChangedEvent(Tile tile, Cell old)
        {
            this.tile = tile;
            this.old = old;
        }
    }
}
