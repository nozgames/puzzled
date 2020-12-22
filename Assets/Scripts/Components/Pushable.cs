using UnityEngine;
using NoZ;

namespace Puzzled
{
    class Pushable : TileComponent
    {
        private Cell moveToCell;
        private Cell moveFromCell;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            SendToCell(new EnterCellEvent(tile, tile.cell), tile.cell);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            var cell = tile.cell;
            if(cell != Cell.invalid)
                SendToCell(new LeaveCellEvent(tile, cell), cell);
        }

        [ActorEventHandler(priority=1)]
        private void OnQueryMove(QueryMoveEvent evt)
        {
            // Do not let anyone move into the same space as the pushable
            evt.result = false;
            evt.IsHandled = true;
        }

        [ActorEventHandler]
        private void OnPush(PushEvent evt) => evt.IsHandled = MoveTo(evt.offset, evt.duration);

        [ActorEventHandler]
        private void OnPull(PullEvent evt) => evt.IsHandled = MoveTo(evt.offset, evt.duration);

        private bool MoveTo(Cell offset, float duration)
        {
            // Check if we can move the same direction as we are being pushed
            var queryMove = new QueryMoveEvent(tile, offset);
            SendToCell(queryMove, queryMove.targetCell, CellEventRouting.FirstVisible);
            if (!queryMove.result)
                return false;

            moveFromCell = tile.cell;
            moveToCell = queryMove.targetCell;

            // Immediately change our cell
            tile.cell = queryMove.targetCell;

            Tween.Move(puzzle.grid.CellToWorld(moveFromCell), puzzle.grid.CellToWorld(moveToCell), false)
                .Duration(duration)
                //.EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(tile.gameObject);
            
            return true;
        }

        private void OnMoveComplete()
        {
            SendToCell(new LeaveCellEvent(tile, moveToCell), moveFromCell);
            SendToCell(new EnterCellEvent(tile, moveFromCell), moveToCell);
        }
    }
}
