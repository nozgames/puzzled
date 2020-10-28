using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class LeaveCellEvent : ActorEvent
    {
        // TODO: need to know who came in and where from

        public LeaveCellEvent Init() {
            return this;
        }
    }
}
