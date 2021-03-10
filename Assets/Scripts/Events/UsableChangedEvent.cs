using NoZ;

namespace Puzzled
{
    public class UsableChangedEvent : ActorEvent
    {
        public Usable source { get; private set; }
        public bool isUsable => source.isUsable;

        public UsableChangedEvent(Usable source)
        {
            this.source = source;
        }
    }
}
