using UnityEngine;
using NoZ;

namespace Puzzled
{
    class Pushable : PuzzledActorComponent
    {
        private Vector2Int moveToCell;

        [ActorEventHandler]
        private void OnQueryMove(QueryMoveEvent evt) => evt.Result = false;

        [ActorEventHandler]
        private void OnQueryPush(QueryPushEvent evt)
        {
            evt.Result = true;
        }

        [ActorEventHandler]
        private void OnPush(PushEvent evt)
        {
            moveToCell = evt.Cell;
            BeginBusy();
            Tween.Move(actor.transform.position, GameManager.CellToWorld(moveToCell), false)
                .Duration(0.4f)
                .EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(actor.gameObject);
        }

        private void OnMoveComplete()
        {
            SendToCell(ActorEvent.Singleton<LeaveCellEvent>().Init(), actor.Cell);
            GameManager.Instance.SetActorCell(actor, moveToCell);
            SendToCell(ActorEvent.Singleton<EnterCellEvent>().Init(), moveToCell);
            
            EndBusy();
        }
    }
}
