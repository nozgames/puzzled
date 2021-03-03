using UnityEngine;
using NoZ;
using UnityEngine.VFX;

namespace Puzzled
{
    class Pushable : TileComponent
    {
        [SerializeField] private AudioClip _moveSound = null;
        [SerializeField] private VisualEffect _vfx = null;

        private Cell moveToCell;
        private Cell moveFromCell;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            SendToCell(new EnterCellEvent(tile, tile.cell), tile.cell);
        }

        [ActorEventHandler]
        private void OnCellChanged(CellChangedEvent evt)
        {
            if (isEditing)
            {
                SendToCell(new LeaveCellEvent(tile, tile.cell), evt.old);
                SendToCell(new EnterCellEvent(tile, evt.old), tile.cell);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if(_vfx != null)
            {
                _vfx.gameObject.SetActive(true);
                _vfx.Stop();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_vfx != null)
            {
                _vfx.Stop();
                _vfx.gameObject.SetActive(false);
            }

            var cell = tile.cell;
            if(cell != Cell.invalid)
                SendToCell(new LeaveCellEvent(tile, cell), cell);
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            evt.user.Send(new GrabEvent(tile.cell));
        }

        [ActorEventHandler(priority=1)]
        private void OnQueryMove(QueryMoveEvent evt)
        {
            // Do not let anyone move into the same space as the pushable
            evt.result = false;
            evt.IsHandled = true;
        }

        [ActorEventHandler]
        private void OnPush(PushEvent evt) => evt.IsHandled = MoveTo(evt.direction, evt.duration);

        [ActorEventHandler]
        private void OnPull(PullEvent evt) => evt.IsHandled = MoveTo(evt.direction, evt.duration);

        private bool MoveTo(Vector2Int direction, float duration)
        {
            if(!tile.CanMove(Cell.DirectionToEdge(direction)))
                return false;

            moveFromCell = tile.cell;
            moveToCell = moveFromCell + direction;

            // Immediately change our cell
            tile.cell = moveToCell;

            PlaySound(_moveSound);

            if (_vfx != null)
            {
                _vfx.transform.localRotation = Quaternion.LookRotation((puzzle.grid.CellToWorld(moveFromCell) - puzzle.grid.CellToWorld(moveToCell)).normalized, Vector3.up);
                _vfx.Play();
            }

            Tween.Move(puzzle.grid.CellToWorld(moveFromCell), puzzle.grid.CellToWorld(moveToCell), false)
                .Duration(duration)
                //.EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(tile.gameObject);
            
            return true;
        }

        private void OnMoveComplete()
        {
            if (_vfx != null)
                _vfx.Stop();

            SendToCell(new LeaveCellEvent(tile, moveToCell), moveFromCell);
            SendToCell(new EnterCellEvent(tile, moveFromCell), moveToCell);
        }
    }
}
