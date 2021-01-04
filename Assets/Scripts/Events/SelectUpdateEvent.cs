using NoZ;

namespace Puzzled
{
    public class SelectUpdateEvent : ActorEvent
    {
        public Select source { get; private set; }
        public int value { get; private set; }

        public SelectUpdateEvent(Select source, int value)
        {
            this.source = source;
            this.value = value;
        }
    }
}
