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
            // Is player?
            if(evt.source == GameManager.player.tile)
                evt.result = false;
        }
    }
}
