using UnityEngine;
using NoZ;

namespace Puzzled
{
    class Pushable : PuzzledActorComponent
    {
        private Vector2Int moveToCell;
        private Vector2Int moveFromCell;

        [ActorEventHandler(priority=1)]
        private void OnQueryMove(QueryMoveEvent evt)
        {
            evt.result = false;
            evt.IsHandled = true;
        }

        [ActorEventHandler]
        private void OnQueryPush(QueryPushEvent evt)
        {
            var queryMove = new QueryMoveEvent(actor, evt.offset);
            SendToCell(queryMove, queryMove.targetCell);
            evt.result = queryMove.result;
        }

        [ActorEventHandler]
        private void OnPush(PushEvent evt)
        {
            moveFromCell = actor.Cell;
            moveToCell = actor.Cell + evt.offset;
            BeginBusy();
            Tween.Move(actor.transform.position, GameManager.CellToWorld(moveToCell), false)
                .Duration(0.4f)
                .EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(actor.gameObject);
        }

        private void OnMoveComplete()
        {
            SendToCell(ActorEvent.Singleton<LeaveCellEvent>().Init(), moveFromCell);
            actor.Cell = moveToCell;
            SendToCell(ActorEvent.Singleton<EnterCellEvent>().Init(), moveToCell);
            
            EndBusy();
        }
    }
}
