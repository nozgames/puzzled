using NoZ;

namespace Puzzled
{
    public class PullEvent : ActorEvent
    {
        public Tile source { get; private set; }
        public Cell offset { get; private set; }
        public float duration { get; private set; }

        public PullEvent(Tile source, Cell offset, float duration)
        {
            this.source = source;
            this.offset = offset;
            this.duration = duration;
        }
    }
}
