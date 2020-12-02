using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class TileComponent : ActorComponent
    {
        public Tile tile => (Tile)base.actor;

        public void SendToCell(ActorEvent evt, Vector2Int cell) => GameManager.Instance.SendToCell(evt, cell);

        protected void BeginBusy() => GameManager.IncBusy();

        protected void EndBusy() => GameManager.DecBusy();
    }
}
