using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Immovable : ActorComponent
    {
        [ActorEventHandler]
        private void OnQueryMove (QueryMoveEvent evt)
        {
            evt.result = false; 
        }
    }
}
