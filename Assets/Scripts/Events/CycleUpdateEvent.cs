using NoZ;

namespace Puzzled
{
    public class CycleUpdateEvent : ActorEvent
    {
        public Cycle source { get; private set; }
        public bool isLooping => source.isLooping;
        public bool isActive => source.shouldBeActive;

        public CycleUpdateEvent(Cycle source)
        {
            this.source = source;
        }
    }
}
