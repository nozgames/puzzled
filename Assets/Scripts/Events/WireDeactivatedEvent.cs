using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class WireDeactivatedEvent : ActorEvent
    {
        public Wire wire { get; private set; }

        public WireDeactivatedEvent(Wire wire)
        {
            this.wire = wire;
        }
    }
}
