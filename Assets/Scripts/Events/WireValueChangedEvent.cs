using NoZ;

namespace Puzzled
{
    public class WireValueChangedEvent : ActorEvent
    {
        public Wire wire { get; private set; }

        public WireValueChangedEvent(Wire wire)
        {
            this.wire = wire;
        }
    }
}
