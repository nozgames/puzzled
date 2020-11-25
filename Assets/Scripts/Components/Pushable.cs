using UnityEngine;
using NoZ;

namespace Puzzled
{
    class Pushable : TileComponent
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
            var queryMove = new QueryMoveEvent(tile, evt.offset);
            SendToCell(queryMove, queryMove.targetCell);
            evt.result = queryMove.result;
        }

        [ActorEventHandler]
        private void OnPush(PushEvent evt)
        {
            moveFromCell = tile.cell;
            moveToCell = tile.cell + evt.offset;
            BeginBusy();
            Tween.Move(tile.transform.position, GameManager.CellToWorld(moveToCell), false)
                .Duration(evt.duration)
                //.EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(tile.gameObject);
        }

        private void OnMoveComplete()
        {
            SendToCell(ActorEvent.Singleton<LeaveCellEvent>().Init(), moveFromCell);
            tile.cell = moveToCell;
            SendToCell(ActorEvent.Singleton<EnterCellEvent>().Init(), moveToCell);
            
            EndBusy();
        }
    }
}
