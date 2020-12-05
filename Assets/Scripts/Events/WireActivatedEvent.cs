using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class WireActivatedEvent : ActorEvent
    {
        public Wire wire { get; private set; }

        public WireActivatedEvent (Wire wire)
        {
            this.wire = wire;
        }
    }
}
