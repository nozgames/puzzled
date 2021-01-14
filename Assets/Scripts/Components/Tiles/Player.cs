using System;
using UnityEngine;
using UnityEngine.InputSystem;
using NoZ;

namespace Puzzled
{
    public class Player : TileComponent
    {
        private readonly Cell leftCell = new Cell(-1, 0);
        private readonly Cell rightCell = new Cell(1, 0);
        private readonly Cell upCell = new Cell(0, 1);
        private readonly Cell downCell = new Cell(0, -1);

        private Cell moveFromCell;
        private Cell moveToCell;
        private float queuedMoveTime = float.MinValue;

        public Tile inventory { get; private set; }

        [SerializeField] private float queuedInputThreshold = 0.25f;
        [SerializeField] private float moveDuration = 0.4f;

        [SerializeField] private GameObject visualsLeft = null;

        [SerializeField] private InputActionReference leftAction = null;
        [SerializeField] private InputActionReference rightAction = null;
        [SerializeField] private InputActionReference upAction = null;
        [SerializeField] private InputActionReference downAction = null;
        [SerializeField] private InputActionReference useAction = null;

        [SerializeField] private Animator _animator = null;

        private bool isGrabbing = false;

        private bool isLeftHeld = false;
        private bool isRightHeld = false;
        private bool isUpHeld = false;
        private bool isDownHeld = false;
        private bool isUseHeld = false;

        private Cell facingDirection; // this is the direction the player is facing
        private Cell desiredMovement; // this is the currently held input
        private Cell queuedMovement;  // this is the last desired movement (if within input threshold it will be treated as desired)
        private Cell lastMovement;  // this is the last continuous movement made (cleared when desired is cleared)

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
            base.OnDisable();

            var cell = tile.cell;
            if (cell != Cell.invalid)
                SendToCell(new LeaveCellEvent(tile, cell), cell);

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

            isLeftHeld = false;
            isRightHeld = false;
            isUpHeld = false;
            isDownHeld = false;
            isUseHeld = false;
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            SendToCell(new EnterCellEvent(tile, tile.cell), tile.cell);

            // TODO: option for intial facing
            visualsLeft.SetActive(true);
            //visualsRight.SetActive(false);
            //visualsUp.SetActive(false);
            //visualsDown.SetActive(false);            
        }

        [ActorEventHandler]
        private void OnUpdate(NoZ.ActorUpdateEvent evt)
        {
            // update queued movement state
            if ((desiredMovement != Cell.zero) && (desiredMovement != lastMovement))
            {
                queuedMovement = desiredMovement;
                queuedMoveTime = Time.time;
            }

            // timeout movement if it has been too long
            if (Time.time > (queuedMoveTime + queuedInputThreshold))
                queuedMovement = Cell.zero;

            if (GameManager.isBusy)
                return;

            // handle use? TODO

            // handle move
            Cell movement = desiredMovement;
            if (movement == Cell.zero)
                movement = queuedMovement;

            if (movement != Cell.zero)
            {
                PerformMove(movement);
                lastMovement = movement;

                // if there is no desired movement anymore, clear the queued movement too
                if (desiredMovement == Cell.zero)
                    queuedMovement = Cell.zero;
            } else 
                _animator.SetBool("Walking", false);
        }

        [ActorEventHandler]
        private void OnGrab(GrabEvent evt)
        {
            Debug.Assert(evt.target.IsAdjacentTo(tile.cell));

            Cell targetDirection = evt.target - tile.cell;
            UpdateVisuals(targetDirection);

            isGrabbing = true;

            _animator.SetBool("Grabbing", isGrabbing);
        }

        private void OnLeftActionStarted(InputAction.CallbackContext ctx)
        {
            isLeftHeld = !KeyboardManager.isShiftPressed;
            if (!isLeftHeld && !isGrabbing)
                UpdateVisuals(Cell.left);
            else
                desiredMovement = leftCell;
        }

        private void OnRightActionStarted(InputAction.CallbackContext ctx)
        {
            isRightHeld = !KeyboardManager.isShiftPressed;
            if (!isRightHeld && !isGrabbing)
                UpdateVisuals(Cell.right);
            else
                desiredMovement = rightCell;
        }

        private void OnUpActionStarted(InputAction.CallbackContext ctx)
        {
            isUpHeld = !KeyboardManager.isShiftPressed;
            if (!isUpHeld && !isGrabbing)
                UpdateVisuals(Cell.up);
            else
                desiredMovement = upCell;
        }

        private void OnDownActionStarted(InputAction.CallbackContext ctx)
        {
            isDownHeld = !KeyboardManager.isShiftPressed;
            if (!isDownHeld && !isGrabbing)
                UpdateVisuals(Cell.down);
            else
                desiredMovement = downCell;
        }

        private void OnLeftActionEnded(InputAction.CallbackContext ctx)
        {
            isLeftHeld = false;
            if (desiredMovement == leftCell)
            {
                desiredMovement = Cell.zero;
                UpdateDesiredMovement();
            }
        }

        private void OnRightActionEnded(InputAction.CallbackContext ctx)
        {
            isRightHeld = false;
            if (desiredMovement == rightCell)
            { 
                desiredMovement = Cell.zero;
                UpdateDesiredMovement();
            }

        }

        private void OnUpActionEnded(InputAction.CallbackContext ctx)
        {
            isUpHeld = false;
            if (desiredMovement == upCell)
            { 
                desiredMovement = Cell.zero;
                UpdateDesiredMovement();
            }
        }

        private void OnDownActionEnded(InputAction.CallbackContext ctx)
        {
            isDownHeld = false;
            if (desiredMovement == downCell)
            { 
                desiredMovement = Cell.zero;
                UpdateDesiredMovement();
            }
        }

        private void OnUseActionStarted(InputAction.CallbackContext ctx) => isUseHeld = true;
        private void OnUseActionEnded(InputAction.CallbackContext ctx)
        {
            isUseHeld = isGrabbing = false;
            _animator.SetBool("Grabbing", false);
        }

        private void OnUseAction(InputAction.CallbackContext ctx)
        {
            if (facingDirection == Cell.zero)
                return; // use needs a direction

            PerformUse(facingDirection);
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
                lastMovement = Cell.zero;
        }

        private void PerformMove(Cell cell)
        {
            queuedMoveTime = float.MinValue;

            if (isGrabbing)
            {
                // Try a push move
                if (PushMove(cell))
                    return;

                // Try a pull move
                PullMove(cell);

                return;  // block other movement
            }

            // Change facing direction 
            /*            if (cell.x < 0)
                            visuals.localScale = new Vector3(-1, 1, 1);
                        else if (cell.x > 0)
                            visuals.localScale = Vector3.one;
            */
            UpdateVisuals(cell);

            if (Move(cell))
                return;
        }

        private void PerformUse(Cell cell)
        {
            if (GameManager.isBusy)
                return;

            queuedMoveTime = float.MinValue;

            // Change facing direction 
/*            if (cell.x < 0)
                visuals.localScale = new Vector3(-1, 1, 1);
            else if (cell.x > 0)
                visuals.localScale = Vector3.one;
*/
            // Try a use move
            if (Use(cell))
                return;
        }

        private bool Move (Cell offset)
        {
            moveFromCell = tile.cell;
            moveToCell = tile.cell + offset;

            // Can we move wherre we are going?
            var query = new QueryMoveEvent(tile, offset);
            SendToCell(query, moveToCell, CellEventRouting.FirstVisible);
            if (!query.result)
                return false;

            // Move to that cell immediately 
            tile.cell = moveToCell;

            // HACK: the tile.cell call moved the players position but we use root animation to move the player so move him back
            tile.transform.position = puzzle.grid.CellToWorld(moveFromCell);

            BeginBusy();

            _animator.SetBool("Walking", true);

            Tween.Move(puzzle.grid.CellToWorld(moveFromCell), puzzle.grid.CellToWorld(moveToCell), false)
            //Tween.Wait(moveDuration)
                .Duration(moveDuration)
                .OnStop(OnMoveComplete)
                .Start(gameObject);

            return true;
        }

        private bool PushMove (Cell offset)
        {
            Debug.Assert(isGrabbing);

            if (facingDirection != offset)
                return false; // can only push in direction of grabbed object

            moveFromCell = tile.cell;
            moveToCell = tile.cell + offset;

            // First tell the tile in the push direction to push.  If a tile pushed it will set IsHandled to true
            if (!SendToCell(new PushEvent(tile, offset, moveDuration), moveToCell, CellEventRouting.FirstVisible))
                return false;

            // Move to the new cell immediately to ensure proper timing.  This will teleport the player
            // but the call to Tween.Move below will immediately set the player back to the previous position
            // to play the animation.
            tile.cell = moveToCell;

            BeginBusy();

            PlayAnimation("Push");

            Tween.Move(puzzle.grid.CellToWorld(moveFromCell), puzzle.grid.CellToWorld(moveToCell), false)
                .Duration(moveDuration)
                .OnStop(OnMoveComplete)
                .Start(tile.gameObject);

            return true;
        }

        private bool PullMove(Cell offset)
        {
            Debug.Assert(isGrabbing);
 
            if (facingDirection != offset.Flipped())
                return false; // can only pull in opposite direction of grabbed object

            moveFromCell = tile.cell;
            moveToCell = tile.cell + offset;
            var pullFromCell = tile.cell - offset;

            // Make sure we can actually move where we are going first
            var queryMove = new QueryMoveEvent(tile, moveToCell);
            SendToCell(queryMove, moveToCell, CellEventRouting.FirstVisible);
            if (!queryMove.result)
                return false;

            // Move to the new cell immediately to ensure proper timing.  This will teleport the player
            // but the call to Tween.Move below will immediately set the player back to the previous position
            // to play the animation.
            tile.cell = moveToCell;

            // Pull in the direction
            if (!SendToCell(new PullEvent(tile, offset, moveDuration), pullFromCell, CellEventRouting.FirstVisible))
            {
                // Move back since the pull failed
                tile.cell = moveFromCell;
                return false;
            }                

            BeginBusy();

            PlayAnimation("Push"); // FIXME: we need a pull

            // Animate the movement
            Tween.Move(puzzle.grid.CellToWorld(moveFromCell), puzzle.grid.CellToWorld(moveToCell), false)
                .Duration(moveDuration)
                //                .EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(tile.gameObject);

            return true;
        }

        private bool Use (Cell offset)
        {
            return SendToCell(new UseEvent(tile), tile.cell + offset, CellEventRouting.FirstVisible);
        }

        private void OnMoveComplete()
        {
            // Send events to cells to let them know move is finished
            SendToCell(new LeaveCellEvent(actor, moveToCell), moveFromCell);
            SendToCell(new EnterCellEvent(actor, moveFromCell), moveToCell);

            //_animator.SetBool("Walking", false);

            EndBusy();
        }

        private void PlayAnimation (string name)
        {
            //animator.SetTrigger(name);
        }

        [ActorEventHandler]
        private void OnLevelExit (LevelExitEvent evt)
        {
            BeginBusy();
            PlayAnimation("Exit");

            Tween.Wait(2.0f).OnStop(() => GameManager.PuzzleComplete()).Start(gameObject);
        }        

        [ActorEventHandler]
        private void OnGiveItem (GiveItemEvent evt)
        {
            var drop = inventory;
            if(drop != null)
            {
                // Dont bother picking up the same item
                if (drop.guid == evt.item.tile.guid)
                    return;
            }

            BeginBusy();
            PlayAnimation("Exit");

            var itemVisuals = evt.item.CloneVisuals(transform);
            itemVisuals.transform.localPosition = new Vector3(0, 1.1f, 0);

            var dropCell = evt.item.tile.cell;
            inventory = TileDatabase.GetTile(evt.item.tile.guid);

            // Drop the item in our inventory
            if (drop != null)
            {
                BeginBusy();
                Tween.Wait(0.01f)
                    .OnStop(() => {
                        var dropped = puzzle.InstantiateTile(drop, dropCell);
                       
                        Tween.Scale(0.0f, 1.0f)
                            .Duration(0.05f)
                            .Start(dropped.gameObject);
                        TweenAnimations.ArcMove(puzzle.grid.CellToWorld(tile.cell), puzzle.grid.CellToWorld(dropped.cell), 0.5f)
                            .Duration(0.15f)
                            .OnStop(EndBusy)
                            .Start(dropped.gameObject);
                    })
                    .Start(gameObject);
            }

            Tween.Wait(0.75f).OnStop(() => {
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
            if(inventory != null && evt.itemGuid == inventory.guid)
            {
                evt.IsHandled = true;
            }
        }

        private void UpdateVisuals(Cell cell)
        {
            facingDirection = cell;

            //visualsLeft.SetActive(cell.x < 0);
            //visualsRight.SetActive(cell.x > 0);
            //visualsDown.SetActive(cell.y < 0);
            //visualsUp.SetActive(cell.y > 0);

            if(cell.x == -1)
                visualsLeft.transform.localRotation = Quaternion.Euler(0, -90, 0);
            else if (cell.x == 1)
                visualsLeft.transform.localRotation = Quaternion.Euler(0, 90, 0);
            else if (cell.y == 1)
                visualsLeft.transform.localRotation = Quaternion.Euler(0, 0, 0);
            else if (cell.y == -1)
                visualsLeft.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }
}

