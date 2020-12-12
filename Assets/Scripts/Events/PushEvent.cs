using NoZ;

namespace Puzzled
{
    public class PushEvent : ActorEvent
    {
        public Tile source { get; private set; }
        public Cell offset { get; private set; }
        public float duration { get; private set; }

        public PushEvent (Tile source, Cell offset, float duration)
        {
            this.source = source;
            this.offset = offset;
            this.duration = duration;
        }
    }
}
