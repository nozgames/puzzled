using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LevelExit : PuzzledActorComponent
    {
        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            SendToCell(new LevelExitEvent(), actor.Cell);
            actor.gameObject.SetActive(false);
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
