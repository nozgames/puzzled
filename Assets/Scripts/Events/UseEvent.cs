using UnityEngine;
using NoZ;

namespace Puzzled
{
    class UseEvent : ActorEvent
    {
        public Actor user { get; private set; }

        public UseEvent(Actor user)
        {
            this.user = user;
        }
    }
}
