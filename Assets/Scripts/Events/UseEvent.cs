using UnityEngine;
using NoZ;

namespace Puzzled
{
    class UseEvent : ActorEvent
    {
        public PuzzledActor user { get; private set; }

        public UseEvent(PuzzledActor user)
        {
            this.user = user;
        }
    }
}
