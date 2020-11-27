using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class ActivateWireEvent : ActorEvent
    {
        public Wire wire { get; private set; }

        public ActivateWireEvent (Wire wire)
        {
            this.wire = wire;
        }
    }
}
