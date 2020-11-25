using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LevelExit : TileComponent
    {
        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            SendToCell(new LevelExitEvent(), tile.cell);
            tile.gameObject.SetActive(false);
        }

        [ActorEventHandler]
        private void OnQueryMove(QueryMoveEvent evt)
        {
            // TODO: need better solution for this
            if (!evt.source.GetComponentInChildren<Player>())
                evt.result = false;
        }
    }
}
