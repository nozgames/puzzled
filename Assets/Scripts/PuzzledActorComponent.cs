using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PuzzledActorComponent : ActorComponent
    {
        public new PuzzledActor actor => (PuzzledActor)base.actor;

        public void SendToCell(ActorEvent evt, Vector2Int cell) => GameManager.Instance.SendToCell(evt, cell);

        protected void BeginBusy () 
        {
            // TODO: maintain busy state somewhere?
        }

        protected void EndBusy() 
        {
            // TODO: maintain busy state somewhere
        }
    }
}
