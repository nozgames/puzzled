using System;
using UnityEngine;
using UnityEngine.InputSystem;
using NoZ;
using Puzzled.UI;
using UnityEngine.Scripting;
using Puzzled.Editor;

namespace Puzzled
{
    public class Player : TileComponent
    {
        private const float TooltipDelay = 0.15f;

        private Cell moveFromCell;
        private Cell moveToCell;
        private float queuedMoveTime = float.MinValue;

#if false
        private string _tooltip;
        private Cell _tooltipCell = Cell.invalid;
        private float _tooltipElapsed;
#endif

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

        private CellEdge _desiredMovement; // this is the currently held input
        private CellEdge _lastMovement;  // this is the last continuous movement made (cleared when desired is cleared)
        private CellEdge _queuedMovement;  // this is the last desired movement (if within input threshold it will be treated as desired)

        private CellEdge _facingDirection; // this is the direction the player is facing

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
        private void OnDestroyEvent(DestroyEvent evt)
        {
            UIManager.HideTooltip();
        }

        [ActorEventHandler]
        private void OnUpdate(NoZ.ActorUpdateEvent evt)
        {
            // update queued movement state
            if (_desiredMovement != CellEdge.None && (_desiredMovement != _lastMovement))
            {
                _queuedMovement = _desiredMovement;
                queuedMoveTime = Time.time;
            }

            // timeout movement if it has been too long
            if (Time.time > (queuedMoveTime + queuedInputThreshold))
                _queuedMovement = CellEdge.None;

            if (GameManager.isBusy)
                return;

            UpdateTooltip();

            // handle use? TODO

            // handle move
            CellEdge edge = _desiredMovement;
            if (edge == CellEdge.None)
                edge = _queuedMovement;

            if (edge != CellEdge.None)
            {
                PerformMove(GetYawRotatedEdge(edge));
                _lastMovement = edge;

                // if there is no desired movement anymore, clear the queued movement too
                if (_desiredMovement == CellEdge.None)
                    _queuedMovement = CellEdge.None;
            } 
            else if (_animator.GetBool("Walking"))
                _animator.SetBool("Walking", false);
        }

        [ActorEventHandler]
        private void OnGrab(GrabEvent evt)
        {
            Debug.Assert(evt.target.IsAdjacentTo(tile.cell));

            UpdateVisuals(Cell.DirectionToEdge(evt.target - tile.cell));

            isGrabbing = true;

            _animator.SetBool("Grabbing", isGrabbing);
            UIManager.HideTooltip();
        }

        private CellEdge GetYawRotatedEdge(CellEdge edge)
        {
            if (edge == CellEdge.None)
                return edge;

            CellEdge rotatedEdge = (CellEdge)((int)((edge - 1) + CameraManager.yawIndex) % 4) + 1;
            return rotatedEdge;
        }

        private void OnLeftActionStarted(InputAction.CallbackContext ctx)
        {
            isLeftHeld = !KeyboardManager.isShiftPressed;
            if (!isLeftHeld && !isGrabbing)
                UpdateVisuals(GetYawRotatedEdge(CellEdge.West));
            else
                _desiredMovement = CellEdge.West;
        }

        private void OnRightActionStarted(InputAction.CallbackContext ctx)
        {
            isRightHeld = !KeyboardManager.isShiftPressed;
            if (!isRightHeld && !isGrabbing)
                UpdateVisuals(GetYawRotatedEdge(CellEdge.East));
            else
                _desiredMovement = CellEdge.East;
        }

        private void OnUpActionStarted(InputAction.CallbackContext ctx)
        {
            isUpHeld = !KeyboardManager.isShiftPressed;
            if (!isUpHeld && !isGrabbing)
                UpdateVisuals(GetYawRotatedEdge(CellEdge.North));
            else
                _desiredMovement = CellEdge.North;
        }

        private void OnDownActionStarted(InputAction.CallbackContext ctx)
        {
            isDownHeld = !KeyboardManager.isShiftPressed;
            if (!isDownHeld && !isGrabbing)
                UpdateVisuals(GetYawRotatedEdge(CellEdge.South));
            else
                _desiredMovement = CellEdge.South;
        }

        private void OnLeftActionEnded(InputAction.CallbackContext ctx)
        {
            isLeftHeld = false;
            if (_desiredMovement == CellEdge.West)
            {
                _desiredMovement = CellEdge.None;
                UpdateDesiredMovement();
            }
        }

        private void OnRightActionEnded(InputAction.CallbackContext ctx)
        {
            isRightHeld = false;
            if (_desiredMovement == CellEdge.East)
            {
                _desiredMovement = CellEdge.None;
                UpdateDesiredMovement();
            }
        }

        private void OnUpActionEnded(InputAction.CallbackContext ctx)
        {
            isUpHeld = false;
            if (_desiredMovement == CellEdge.North)
            {
                _desiredMovement = CellEdge.None;
                UpdateDesiredMovement();
            }
        }

        private void OnDownActionEnded(InputAction.CallbackContext ctx)
        {
            isDownHeld = false;
            if (_desiredMovement == CellEdge.South)
            {
                _desiredMovement = CellEdge.None;
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
            if (_facingDirection == CellEdge.None)
                return; // use needs a direction

            if (GameManager.isBusy)
                return;

            queuedMoveTime = float.MinValue;

            var cell = tile.cell;
            
            // Try to use the edge in front of us first
            if (SendToCell(new UseEvent(tile), new Cell(CellCoordinateSystem.Edge, cell.x, cell.y, _facingDirection), CellEventRouting.FirstVisible))
                return;

            // Then try to use the shared edge
            if (SendToCell(new UseEvent(tile), new Cell(CellCoordinateSystem.SharedEdge, cell.x, cell.y, _facingDirection), CellEventRouting.FirstVisible))
                return;

            // Before we try to use the adjacent tile make sure there is nothing obstructing our movement on that edge
            // as that would also obstruct our being able to use.
            if (!tile.CanMoveThroughEdge(_facingDirection))
                return;

            // Use the adjacent cell
            SendToCell(new UseEvent(tile), tile.cell + Cell.EdgeToDirection(_facingDirection), CellEventRouting.FirstVisible);
        }

        private void UpdateDesiredMovement()
        {
            if (isLeftHeld)
                _desiredMovement = CellEdge.West;
            else if (isRightHeld)
                _desiredMovement = CellEdge.East;
            else if (isUpHeld)
                _desiredMovement = CellEdge.North;
            else if (isDownHeld)
                _desiredMovement = CellEdge.South;
            else
                _lastMovement = CellEdge.None;
        }

        private void PerformMove(CellEdge edge)
        {
            queuedMoveTime = float.MinValue;

            if (isGrabbing)
            {
                if(Cell.IsOppositeEdge(_facingDirection, edge))
                    PullMove(edge);
                else
                    PushMove(edge);

                return;  // block other movement
            }

            UpdateVisuals(edge);

            if (Move(edge))
                return;
        }

        private bool Move (CellEdge edge)
        {
            if (!tile.CanMove(edge))
                return false;

            var offset = Cell.EdgeToDirection(edge);
            moveFromCell = tile.cell;
            moveToCell = tile.cell + offset;

            // Move to that cell immediately 
            tile.cell = moveToCell;

            // HACK: the tile.cell call moved the players position but we use root animation to move the player so move him back
            tile.transform.position = puzzle.grid.CellToWorld(moveFromCell);

            BeginBusy();

            _animator.SetBool("Walking", true);
            UIManager.HideTooltip();

            Tween.Move(puzzle.grid.CellToWorld(moveFromCell), puzzle.grid.CellToWorld(moveToCell), false)
            //Tween.Wait(moveDuration)
                .Duration(moveDuration)
                .OnStop(OnMoveComplete)
                .Start(gameObject);

            return true;
        }

        public bool Teleport (Cell cell)
        {
            if (cell == tile.cell)
                return true;

            if (puzzle.grid.CellToTile(tile.cell, TileLayer.Dynamic) == null)
                return false;

            var old = tile.cell;

            tile.cell = cell;

            // Send events to cells to let them know move is finished
            SendToCell(new LeaveCellEvent(actor, cell), old);
            SendToCell(new EnterCellEvent(actor, old), cell);

            return true;
        }

        private bool PushMove (CellEdge edge)
        {
            Debug.Assert(isGrabbing);
            Debug.Assert(!Cell.IsOppositeEdge(_facingDirection, edge));

            var offset = Cell.EdgeToDirection(edge);
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

        private bool PullMove(CellEdge edge)
        {
            Debug.Assert(isGrabbing);
            Debug.Assert(Cell.IsOppositeEdge(_facingDirection, edge));

            if (!tile.CanMove(edge))
                return false;

            var offset = Cell.EdgeToDirection(edge);
            moveFromCell = tile.cell;
            moveToCell = tile.cell + offset;
            var pullFromCell = tile.cell - offset;

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

        private bool Use (CellEdge edge)
        {
            var direction = Cell.EdgeToDirection(edge);

            // Try to use the edge in front of us first
            if (SendToCell(new UseEvent(tile), new Cell(tile.cell, Cell.DirectionToEdge(direction)), CellEventRouting.FirstVisible))
                return true;

            // Before we try to use what is in front of us make sure there isnt anything on the edge that would prevent it
            if (!tile.CanMoveThroughEdge(edge))
                return false;

            return SendToCell(new UseEvent(tile), tile.cell + direction, CellEventRouting.FirstVisible);
        }

        private void OnMoveComplete()
        {
            // Send events to cells to let them know move is finished
            SendToCell(new LeaveCellEvent(actor, moveToCell), moveFromCell);
            SendToCell(new EnterCellEvent(actor, moveFromCell), moveToCell);

            //_animator.SetBool("Walking", false);

            EndBusy();

            // If the game manager is busy then we need to return to idle 
            // because we will not be able to move anyway
            if(GameManager.isBusy)
                _animator.SetBool("Walking", false);
        }

        private void HideTooltip()
        {
#if false
            _tooltip = null;
            _tooltipCell = Cell.invalid;
            UIManager.HideTooltip();
#endif
        }

        private void UpdateTooltip()
        {
#if false
            if(_animator.GetBool("Walking") || isGrabbing)
            {
                HideTooltip();
                return;
            }

            var cell = tile.cell;
            var edgeCell = new Cell(CellCoordinateSystem.Edge, cell.x, cell.y, _facingDirection);
            var direction = Cell.EdgeToDirection (_facingDirection);

            var query = new QueryTooltipEvent(this);
            SendToCell(query, edgeCell, CellEventRouting.FirstHandled);
            if (query.tooltip == null)
            {
                cell += direction;

                // Make sure there is nothing blocking the tooltip
                if (!tile.CanMoveThroughEdge(_facingDirection))
                    return;

                SendToCell(query, cell, CellEventRouting.FirstHandled);
                if (query.tooltip == null)
                {
                    HideTooltip();
                    return;
                }
            } else
                cell = edgeCell;

            if (_tooltipCell == cell && query.tooltip == _tooltip)
            {
                if (_tooltipElapsed < TooltipDelay)
                {
                    _tooltipElapsed += Time.deltaTime;
                    if (_tooltipElapsed >= TooltipDelay)
                    {
                        _tooltipElapsed = TooltipDelay;
                        var tooltipDirection = TooltipDirection.Top;
                        var worldOffset = new Vector3(0, 0, 0.25f);
                        UIManager.ShowTooltip(puzzle.grid.CellToWorldBounds(cell).center + worldOffset + Vector3.up * query.height, query.tooltip, tooltipDirection);
                    }
                }
            } 
            else
            {
                _tooltip = query.tooltip;
                _tooltipCell = cell;
                _tooltipElapsed = 0.0f;
            }
#endif
        }

        private void PlayAnimation (string name)
        {
            //animator.SetTrigger(name);
        }

        [ActorEventHandler]
        private void OnLevelExit (LevelExitEvent evt)
        {
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
            inventory = DatabaseManager.GetTile(evt.item.tile.guid);

            UIManager.SetPlayerItem(inventory);

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
        private void OnLevelExitEvent (LevelExitEvent evt)
        {
            BeginBusy();
            PlayAnimation("Exit");

            var itemVisuals = evt.exit.CloneVisuals(transform);
            itemVisuals.transform.localPosition = new Vector3(0, 1.1f, 0);

            Tween.Wait(0.75f).OnStop(() => {
                Destroy(itemVisuals);
                PlayAnimation("Idle");
                EndBusy();

                if (UIPuzzleEditor.isOpen)
                    UIPuzzleEditor.Stop();
                else
                {
                    puzzle.MarkCompleted();
                    GameManager.Stop();
                    GameManager.UnloadPuzzle();
                    UIManager.ShowPlayWorldScreen();
                }

            }).Start(gameObject);

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

        private void UpdateVisuals(CellEdge edge)
        {
            _facingDirection = edge;

            switch (edge)
            {
                case CellEdge.North:
                    visualsLeft.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;

                case CellEdge.South:
                    visualsLeft.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    break;

                case CellEdge.East:
                    visualsLeft.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    break;

                case CellEdge.West:
                    visualsLeft.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    break;
            }
        }
    }
}

