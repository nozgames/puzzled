using NoZ;

namespace Puzzled
{
    public class CycleAdvanceEvent : ActorEvent
    {
        public Cycle source { get; private set; }
        public bool isLooping => source.isLooping;

        public CycleAdvanceEvent(Cycle source)
        {
            this.source = source;
        }
    }
}
