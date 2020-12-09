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
        private float queuedMoveTime = float.MinValue;

        [SerializeField] private float queuedInputThreshold = 0.25f;
        [SerializeField] private float moveDuration = 0.4f;

        [SerializeField] private Transform visuals = null;

        [SerializeField] private InputActionReference leftAction = null;
        [SerializeField] private InputActionReference rightAction = null;
        [SerializeField] private InputActionReference upAction = null;
        [SerializeField] private InputActionReference downAction = null;
        [SerializeField] private InputActionReference useAction = null;

        private bool isLeftHeld = false;
        private bool isRightHeld = false;
        private bool isUpHeld = false;
        private bool isDownHeld = false;
        private bool isUseHeld = false;

        private Vector2Int desiredMovement; // this is the currently held input
        private Vector2Int queuedMovement;  // this is the last desired movement (if within input threshold it will be treated as desired)
        private Vector2Int lastMovement;  // this is the last continuous movement made (cleared when desired is cleared)

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
            useAction.action.Enable();

            leftAction.action.started += OnLeftActionStarted;
            rightAction.action.started += OnRightActionStarted;
            upAction.action.started += OnUpActionStarted;
            downAction.action.started += OnDownActionStarted;
            leftAction.action.canceled += OnLeftActionEnded;
            rightAction.action.canceled += OnRightActionEnded;
            upAction.action.canceled += OnUpActionEnded;
            downAction.action.canceled += OnDownActionEnded;
            useAction.action.started += OnUseActionStarted;
            useAction.action.performed += OnUseAction;
            useAction.action.canceled += OnUseActionEnded;
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
            useAction.action.started -= OnUseActionStarted;
            useAction.action.performed -= OnUseAction;
            useAction.action.canceled -= OnUseActionEnded;

            leftAction.action.Disable();
            rightAction.action.Disable();
            upAction.action.Disable();
            downAction.action.Disable();
            useAction.action.Disable();

            base.OnDisable();
        }

        [ActorEventHandler]
        private void OnUpdate(NoZ.ActorUpdateEvent evt)
        {
            // update queued movement state
            if ((desiredMovement != Vector2Int.zero) && (desiredMovement != lastMovement))
            {
                queuedMovement = desiredMovement;
                queuedMoveTime = Time.time;
            }

            // timeout movement if it has been too long
            if (Time.time > (queuedMoveTime + queuedInputThreshold))
                queuedMovement = Vector2Int.zero;

            if (GameManager.IsBusy)
                return;

            // handle use? TODO

            // handle move
            Vector2Int movement = desiredMovement;
            if (movement == Vector2Int.zero)
                movement = queuedMovement;

            if (movement != Vector2Int.zero)
            {
                PerformMove(movement);
                lastMovement = movement;

                // if there is no desired movement anymore, clear the queued movement too
                if (desiredMovement == Vector2Int.zero)
                    queuedMovement = Vector2Int.zero;
            }
        }

        private void OnLeftActionStarted(InputAction.CallbackContext ctx)
        {
            isLeftHeld = true;
            desiredMovement = leftCell;
        }

        private void OnRightActionStarted(InputAction.CallbackContext ctx)
        {
            isRightHeld = true;
            desiredMovement = rightCell;
        }

        private void OnUpActionStarted(InputAction.CallbackContext ctx)
        {
            isUpHeld = true;
            desiredMovement = upCell;
        }

        private void OnDownActionStarted(InputAction.CallbackContext ctx)
        {
            isDownHeld = true;
            desiredMovement = downCell;
        }

        private void OnLeftActionEnded(InputAction.CallbackContext ctx)
        {
            isLeftHeld = false;
            if (desiredMovement == leftCell)
            {
                desiredMovement = Vector2Int.zero;
                UpdateDesiredMovement();
            }
        }

        private void OnRightActionEnded(InputAction.CallbackContext ctx)
        {
            isRightHeld = false;
            if (desiredMovement == rightCell)
            { 
                desiredMovement = Vector2Int.zero;
                UpdateDesiredMovement();
            }

        }

        private void OnUpActionEnded(InputAction.CallbackContext ctx)
        {
            isUpHeld = false;
            if (desiredMovement == upCell)
            { 
                desiredMovement = Vector2Int.zero;
                UpdateDesiredMovement();
            }
        }

        private void OnDownActionEnded(InputAction.CallbackContext ctx)
        {
            isDownHeld = false;
            if (desiredMovement == downCell)
            { 
                desiredMovement = Vector2Int.zero;
                UpdateDesiredMovement();
            }
        }

        private void OnUseActionStarted(InputAction.CallbackContext ctx) => isUseHeld = true;
        private void OnUseActionEnded(InputAction.CallbackContext ctx) => isUseHeld = false;

        private void OnUseAction(InputAction.CallbackContext ctx)
        {
            if (desiredMovement == Vector2Int.zero)
                return; // use needs a direction

            PerformUse(desiredMovement);
        }

        private void UpdateDesiredMovement()
        {
            if (isLeftHeld)
                desiredMovement = leftCell;
            else if (isRightHeld)
                desiredMovement = rightCell;
            else if (isUpHeld)
                desiredMovement = upCell;
            else if (isDownHeld)
                desiredMovement = downCell;
            else
                lastMovement = Vector2Int.zero;
        }

        private void PerformMove(Vector2Int cell)
        {
            queuedMoveTime = float.MinValue;

            // Change facing direction 
            if (cell.x < 0)
                visuals.localScale = new Vector3(-1, 1, 1);
            else if (cell.x > 0)
                visuals.localScale = Vector3.one;

            if (isUseHeld)
            {
                // Try a push move
                if (PushMove(cell))
                    return;

                // Try a pull move
                if (PullMove(cell))
                {
                    // flip him back to face the pulled object
                    visuals.localScale.Scale(new Vector3(-1, 1, 1));
                    return;
                }
            }

            if (Move(cell))
                return;
        }

        private void PerformUse(Vector2Int cell)
        {
            queuedMoveTime = float.MinValue;

            // Change facing direction 
            if (cell.x < 0)
                visuals.localScale = new Vector3(-1, 1, 1);
            else if (cell.x > 0)
                visuals.localScale = Vector3.one;

            // Try a use move
            if (Use(cell))
                return;
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

        private bool PullMove(Vector2Int offset)
        {
            moveFromCell = tile.cell;
            moveToCell = tile.cell + offset;
            Vector2Int pullFromCell = tile.cell - offset;

            var queryMove = new QueryMoveEvent(tile, offset);
            GameManager.Instance.SendToCell(queryMove, moveToCell);
            if (!queryMove.result)
                return false;

            var query = new QueryPullEvent(tile, offset);
            GameManager.Instance.SendToCell(query, pullFromCell);
            if (!query.result)
                return false;

            BeginBusy();

            PlayAnimation("Push"); // FIXME: we need a pull

            GameManager.Instance.SendToCell(new PullEvent(tile, offset, moveDuration), pullFromCell);

            Tween.Move(tile.transform.position, GameManager.CellToWorld(moveToCell), false)
                .Duration(moveDuration)
                //                .EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(tile.gameObject);

            return true;
        }

        private bool Use (Vector2Int offset)
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

            return true;
        }

        private void OnMoveComplete()
        {
            SendToCell(ActorEvent.Singleton<LeaveCellEvent>().Init(), moveFromCell);
            tile.cell = moveToCell;
            SendToCell(new EnterCellEvent(tile), moveToCell);

            PlayAnimation("Idle");

            EndBusy();
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

