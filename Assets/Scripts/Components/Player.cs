using UnityEngine;
using UnityEngine.InputSystem;
using NoZ;

namespace Puzzled
{
    class Player : TileComponent
    {
        private Vector2Int moveFromCell;
        private Vector2Int moveToCell;
        private Animator animator;
        private Vector2Int queuedAction;
        private float queuedActionTime = float.MinValue;

        [SerializeField] private float queuedInputThreshold = 0.25f;
        [SerializeField] private float moveDuration = 0.4f;

        [SerializeField] private Transform visuals = null;

        [SerializeField] private InputActionReference leftAction = null;
        [SerializeField] private InputActionReference rightAction = null;
        [SerializeField] private InputActionReference upAction = null;
        [SerializeField] private InputActionReference downAction = null;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            leftAction.action.Enable();
            rightAction.action.Enable();
            upAction.action.Enable();
            downAction.action.Enable();

            leftAction.action.performed += OnLeftAction;
            rightAction.action.performed += OnRightAction;
            upAction.action.performed += OnUpAction;
            downAction.action.performed += OnDownAction;
        }

        protected override void OnDisable()
        {
            leftAction.action.performed -= OnLeftAction;
            rightAction.action.performed -= OnRightAction;
            upAction.action.performed -= OnUpAction;
            downAction.action.performed -= OnDownAction;

            base.OnDisable();
        }

        private void OnLeftAction(InputAction.CallbackContext ctx) => PerformAction(new Vector2Int(-1, 0));
        private void OnRightAction(InputAction.CallbackContext ctx) => PerformAction(new Vector2Int(1, 0));
        private void OnUpAction(InputAction.CallbackContext ctx) => PerformAction(new Vector2Int(0, 1));
        private void OnDownAction(InputAction.CallbackContext ctx) => PerformAction(new Vector2Int(0, -1));

        private void PerformAction (Vector2Int cell)
        {
            if (GameManager.IsBusy) 
            {
                queuedAction = cell;
                queuedActionTime = Time.time;
                return;
            }

            queuedActionTime = float.MinValue;

            // Change facing direction 
            if (cell.x < 0)
                visuals.localScale = new Vector3(-1, 1, 1);
            else if (cell.x > 0)
                visuals.localScale = Vector3.one;

            // Try moving first
            if (Move(cell))
                return;

            // Try a push move
            if (PushMove(cell))
                return;

            // Try a use move
            if (UseMove(cell))
                return;
        }

        private void OnActionComplete ()
        {
            if (Time.time - queuedActionTime <= queuedInputThreshold)
                PerformAction(queuedAction);
        }

        private bool Move (Vector2Int offset)
        {
            moveFromCell = tile.cell;
            moveToCell = tile.cell + offset;

            var query = new QueryMoveEvent(tile, offset);
            GameManager.Instance.SendToCell(query, moveToCell);
            if (!query.result)
                return false;

            BeginBusy();

            PlayAnimation("Walk");

            Tween.Move(tile.transform.position, GameManager.CellToWorld(moveToCell), false)
                .Duration(moveDuration)
                .OnStop(OnMoveComplete)
                .Start(tile.gameObject);

            return true;
        }

        private bool PushMove (Vector2Int offset)
        {
            moveFromCell = tile.cell;
            moveToCell = tile.cell + offset;            

            var query = new QueryPushEvent(tile, offset);
            GameManager.Instance.SendToCell(query, moveToCell);
            if (!query.result)
                return false;

            BeginBusy();

            PlayAnimation("Push");

            GameManager.Instance.SendToCell(new PushEvent(tile, offset, moveDuration), moveToCell);

            Tween.Move(tile.transform.position, GameManager.CellToWorld(moveToCell), false)
                .Duration(moveDuration)
//                .EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(tile.gameObject);

            return true;
        }

        private bool UseMove (Vector2Int offset)
        {
            moveFromCell = tile.cell;
            moveToCell = tile.cell + offset;            

            var query = new QueryUseEvent(tile, offset);
            GameManager.Instance.SendToCell(query, moveToCell);
            if (!query.result)
                return false;

            BeginBusy();

            GameManager.Instance.SendToCell(new UseEvent(tile), moveToCell);

            EndBusy();

            OnActionComplete();

            return true;
        }

        private void OnMoveComplete()
        {
            SendToCell(ActorEvent.Singleton<LeaveCellEvent>().Init(), moveFromCell);
            tile.cell = moveToCell;
            SendToCell(ActorEvent.Singleton<EnterCellEvent>().Init(), moveToCell);

            PlayAnimation("Idle");

            EndBusy();

            OnActionComplete();
        }

        private void PlayAnimation (string name)
        {
            animator.SetTrigger(name);
        }

        [ActorEventHandler]
        private void OnLevelExit (LevelExitEvent evt)
        {
            BeginBusy();
            PlayAnimation("Exit");

            Tween.Wait(2.0f).OnStop(() => GameManager.PuzzleComplete()).Start(gameObject);
        }
    }
}

