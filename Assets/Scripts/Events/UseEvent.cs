using UnityEngine;
using NoZ;

namespace Puzzled
{
    class UseEvent : ActorEvent
    {
        public Tile user { get; private set; }

        public UseEvent(Tile user)
        {
            this.user = user;
        }
    }
}
