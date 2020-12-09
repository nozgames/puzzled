using UnityEngine;
using UnityEngine.InputSystem;
using NoZ;

namespace Puzzled
{
    class Player : TileComponent
    {
        private readonly Vector2Int leftCell = new Vector2Int(-1, 0);
        private readonly Vector2Int rightCell = new Vector2Int(1, 0);
        private readonly Vector2Int upCell = new Vector2Int(0, 1);
        private readonly Vector2Int downCell = new Vector2Int(0, -1);

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
        private bool isLeftHeld = false;
        private bool isRightHeld = false;
        private bool isUpHeld = false;
        private bool isDownHeld = false;
        private Vector2Int pendingMovement;

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

            leftAction.action.started += OnLeftActionStarted;
            rightAction.action.started += OnRightActionStarted;
            upAction.action.started += OnUpActionStarted;
            downAction.action.started += OnDownActionStarted;
            leftAction.action.canceled += OnLeftActionEnded;
            rightAction.action.canceled += OnRightActionEnded;
            upAction.action.canceled += OnUpActionEnded;
            downAction.action.canceled += OnDownActionEnded;
        }

        protected override void OnDisable()
        {
            leftAction.action.started -= OnLeftActionStarted;
            rightAction.action.started -= OnRightActionStarted;
            upAction.action.started -= OnUpActionStarted;
            downAction.action.started -= OnDownActionStarted;
            leftAction.action.canceled -= OnLeftActionEnded;
            rightAction.action.canceled -= OnRightActionEnded;
            upAction.action.canceled -= OnUpActionEnded;
            downAction.action.canceled -= OnDownActionEnded;

            base.OnDisable();
        }

        private void OnLeftActionStarted(InputAction.CallbackContext ctx)
        {
            isLeftHeld = true;
            PotentiallyQueueMove(leftCell);
        }

        private void OnRightActionStarted(InputAction.CallbackContext ctx)
        {
            isRightHeld = true;
            PotentiallyQueueMove(rightCell);
        }

        private void OnUpActionStarted(InputAction.CallbackContext ctx)
        {
            isUpHeld = true;
            PotentiallyQueueMove(upCell);
        }

        private void OnDownActionStarted(InputAction.CallbackContext ctx)
        {
            isDownHeld = true;
            PotentiallyQueueMove(downCell);
        }

        private void OnLeftActionEnded(InputAction.CallbackContext ctx) => isLeftHeld = false;
        private void OnRightActionEnded(InputAction.CallbackContext ctx) => isRightHeld = false;
        private void OnUpActionEnded(InputAction.CallbackContext ctx) => isUpHeld = false;
        private void OnDownActionEnded(InputAction.CallbackContext ctx) => isDownHeld = false;

        [ActorEventHandler]
        private void OnTick(TickEvent evt)
        {
            if (GameManager.IsBusy)
                return;

            // queue up a different move if an action is active 
            if (pendingMovement == Vector2Int.zero)
            {
                if (isLeftHeld)
                    PotentiallyQueueMove(leftCell);
                else if (isRightHeld)
                    PotentiallyQueueMove(rightCell);
                else if (isUpHeld)
                    PotentiallyQueueMove(upCell);
                else if (isDownHeld)
                    PotentiallyQueueMove(downCell);
            }

            // handle pending movements
            if (pendingMovement != Vector2Int.zero)
            {
                Move(pendingMovement);
                pendingMovement = Vector2Int.zero;
            }
        }

        private void PotentiallyClearPendingMovement()
        {
            if (pendingMovement == leftCell)
            {
                if (!isLeftHeld)
                    pendingMovement = Vector2Int.zero;
            }
            else if (pendingMovement == rightCell)
            {
                if (!isRightHeld)
                    pendingMovement = Vector2Int.zero;
            }
            else if (pendingMovement == upCell)
            {
                if (!isUpHeld)
                    pendingMovement = Vector2Int.zero;
            }
            else if (pendingMovement == downCell)
            {
                if (!isDownHeld)
                    pendingMovement = Vector2Int.zero;
            }
        }

        private void PotentiallyQueueMove(Vector2Int movement)
        {
            PotentiallyClearPendingMovement();

            // don't set the move if one is already set
            if (pendingMovement != Vector2Int.zero)
                return;

            pendingMovement = movement;
        }

        private void PerformAction (Vector2Int cell)
        {
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
            SendToCell(new EnterCellEvent(tile), moveToCell);

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

        public Item inventory { get; private set; }

        [ActorEventHandler]
        private void OnGiveItem (GiveItemEvent evt)
        {
            BeginBusy();
            PlayAnimation("Exit");

            var itemVisuals = evt.item.CloneVisuals(transform);
            itemVisuals.transform.localPosition = new Vector3(0, 1.1f, 0);

            inventory = evt.item;

            Tween.Wait(1f).OnStop(() => {
                Destroy(itemVisuals);
                PlayAnimation("Idle");
                EndBusy();
            }).Start(gameObject);

            // Indicate we took the item
            evt.IsHandled = true;
        }

        [ActorEventHandler]
        private void OnQueryHasItem (QueryHasItemEvent evt)
        {
            if(inventory != null && evt.itemGuid == inventory.tile.guid)
            {
                evt.IsHandled = true;
            }
        }
    }
}

