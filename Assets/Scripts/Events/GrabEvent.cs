using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class GrabEvent : ActorEvent
    {
        public Cell target { get; private set; }

        public GrabEvent(Cell target)
        {
            this.target = target;
        }
    }
}
