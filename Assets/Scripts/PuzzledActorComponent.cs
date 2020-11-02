using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PuzzledActorComponent : ActorComponent
    {
        public new PuzzledActor actor => (PuzzledActor)base.actor;

        public void SendToCell(ActorEvent evt, Vector2Int cell) => GameManager.Instance.SendToCell(evt, cell);

        protected void BeginBusy() => GameManager.IncBusy();

        protected void EndBusy() => GameManager.DecBusy();
    }
}
