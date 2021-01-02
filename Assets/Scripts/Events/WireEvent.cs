using NoZ;

namespace Puzzled
{
    public class WireEvent : ActorEvent
    {
        public Wire wire { get; private set; }
        public Port port => wire.to.port;

        public WireEvent(Wire wire)
        {
            this.wire = wire;
        }
    }
}
