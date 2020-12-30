using NoZ;

namespace Puzzled
{
    public class CycleResetEvent : ActorEvent
    {
        public Cycle source { get; private set; }
        public bool isLooping => source.isLooping;

        public CycleResetEvent(Cycle source)
        {
            this.source = source;
        }
    }
}
