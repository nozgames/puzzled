using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class DeactivateWireEvent : ActorEvent
    {
        public Wire wire { get; private set; }

        public DeactivateWireEvent(Wire wire)
        {
            this.wire = wire;
        }
    }
}
