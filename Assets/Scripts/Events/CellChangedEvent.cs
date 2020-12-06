using NoZ;

namespace Puzzled
{
    class CellChangedEvent : ActorEvent
    {
        public Tile tile { get; set; }

        public CellChangedEvent(Tile tile)
        {
            this.tile = tile;
        }
    }
}
