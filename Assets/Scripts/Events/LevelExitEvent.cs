using NoZ;
using UnityEngine;

namespace Puzzled
{ 
    public class LevelExitEvent : ActorEvent
    {
        public LevelExit exit { get; private set; }

        public LevelExitEvent(LevelExit exit)
        {
            this.exit = exit;
        }
    }
}
