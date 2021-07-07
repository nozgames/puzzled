using NoZ;
using Puzzled.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzled
{
    public class TilePlayerController : PlayerController
    {
        [SerializeField] private InputActionReference leftAction = null;
        [SerializeField] private InputActionReference rightAction = null;
        [SerializeField] private InputActionReference upAction = null;
        [SerializeField] private InputActionReference downAction = null;

        [SerializeField] private float queuedInputThreshold = 0.25f;
        [SerializeField] private float moveDuration = 0.4f;

        private float queuedMoveTime = float.MinValue;

        private Cell _moveFromCell;
        private Cell _moveToCell;

        private bool isLeftHeld = false;
        private bool isRightHeld = false;
        private bool isUpHeld = false;
        private bool isDownHeld = false;
        private bool isUseHeld = false;
        private bool isGrabbing = false;

        private CellEdge _desiredMovement; // this is the currently held input
        private CellEdge _lastMovement;  // this is the last continuous movement made (cleared when desired is cleared)
        private CellEdge _queuedMovement;  // this is the last desired movement (if within input threshold it will be treated as desired)

        private CellEdge _facingDirection; // this is the direction the player is facing

        public override void HandleEnable(Player player)
        {
            base.HandleEnable(player);

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

        public override void HandleDisable()
        {
            isLeftHeld = false;
            isRightHeld = false;
            isUpHeld = false;
            isDownHeld = false;
            isUseHeld = false;

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
        }

        public override void PreMove()
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
        }

        public override void Move()
        {
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
            else if (player.animator.GetBool("Walking"))
                player.animator.SetBool("Walking", false);
        }

        public override void HandleGrabStarted(GrabEvent evt)
        {
            UpdateVisuals(Cell.DirectionToEdge(evt.target - player.tile.cell));

            isGrabbing = true;

            player.animator.SetBool("Grabbing", isGrabbing);
        }

        public override void UpdateRotation()
        {
            UpdateVisuals((CellEdge)(player.rotation + 1));
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
            player.animator.SetBool("Grabbing", false);
        }

        private void OnUseAction(InputAction.CallbackContext ctx)
        {
            if (_facingDirection == CellEdge.None)
                return; // use needs a direction

            if (GameManager.isBusy)
                return;

            queuedMoveTime = float.MinValue;

            var cell = player.tile.cell;

            // Try to use the edge in front of us first
            if (player.SendToCell(new UseEvent(player.tile), new Cell(CellCoordinateSystem.Edge, cell.x, cell.y, _facingDirection), CellEventRouting.FirstVisible))
                return;

            // Then try to use the shared edge
            if (player.SendToCell(new UseEvent(player.tile), new Cell(CellCoordinateSystem.SharedEdge, cell.x, cell.y, _facingDirection), CellEventRouting.FirstVisible))
                return;

            // Before we try to use the adjacent tile make sure there is nothing obstructing our movement on that edge
            // as that would also obstruct our being able to use.
            if (!player.tile.CanMoveThroughEdge(_facingDirection))
                return;

            // Use the adjacent cell
            player.SendToCell(new UseEvent(player.tile), player.tile.cell + Cell.EdgeToDirection(_facingDirection), CellEventRouting.FirstVisible);
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
                if (Cell.IsOppositeEdge(_facingDirection, edge))
                    PullMove(edge);
                else
                    PushMove(edge);

                return;  // block other movement
            }

            UpdateVisuals(edge);

            if (Move(edge))
                return;
        }

        private bool Move(CellEdge edge)
        {
            if (!player.tile.CanMove(edge))
                return false;

            var offset = Cell.EdgeToDirection(edge);
            _moveFromCell = player.tile.cell;
            _moveToCell = player.tile.cell + offset;

            // Move to that cell immediately 
            player.tile.cell = _moveToCell;

            // HACK: the tile.cell call moved the players position but we use root animation to move the player so move him back
            player.tile.transform.position = player.puzzle.grid.CellToWorld(_moveFromCell);

            GameManager.busy++;

            player.animator.SetBool("Walking", true);
            UIManager.HideTooltip();

            Tween.Move(player.puzzle.grid.CellToWorld(_moveFromCell), player.puzzle.grid.CellToWorld(_moveToCell), false)
            //Tween.Wait(moveDuration)
                .Duration(moveDuration)
                .OnStop(OnMoveComplete)
                .Start(gameObject);

            return true;
        }

        private void OnMoveComplete()
        {
            // Send events to cells to let them know move is finished
            player.SendToCell(new LeaveCellEvent(player.actor, _moveToCell), _moveFromCell);
            player.SendToCell(new EnterCellEvent(player.actor, _moveFromCell), _moveToCell);

            //_animator.SetBool("Walking", false);

            GameManager.busy--;

            // If the game manager is busy then we need to return to idle 
            // because we will not be able to move anyway
            if (GameManager.isBusy)
                player.animator.SetBool("Walking", false);
        }

        private bool PushMove(CellEdge edge)
        {
            Debug.Assert(isGrabbing);
            Debug.Assert(!Cell.IsOppositeEdge(_facingDirection, edge));

            var offset = Cell.EdgeToDirection(edge);
            _moveFromCell = player.tile.cell;
            _moveToCell = player.tile.cell + offset;

            // First tell the tile in the push direction to push.  If a tile pushed it will set IsHandled to true
            if (!player.SendToCell(new PushEvent(player.tile, offset, moveDuration), _moveToCell, CellEventRouting.FirstVisible))
                return false;

            // Move to the new cell immediately to ensure proper timing.  This will teleport the player
            // but the call to Tween.Move below will immediately set the player back to the previous position
            // to play the animation.
            player.tile.cell = _moveToCell;

            GameManager.busy++;

            player.PlayAnimation("Push");

            Tween.Move(player.puzzle.grid.CellToWorld(_moveFromCell), player.puzzle.grid.CellToWorld(_moveToCell), false)
                .Duration(moveDuration)
                .OnStop(OnMoveComplete)
                .Start(player.tile.gameObject);

            return true;
        }

        private bool PullMove(CellEdge edge)
        {
            Debug.Assert(isGrabbing);
            Debug.Assert(Cell.IsOppositeEdge(_facingDirection, edge));

            if (!player.tile.CanMove(edge))
                return false;

            var offset = Cell.EdgeToDirection(edge);
            _moveFromCell = player.tile.cell;
            _moveToCell = player.tile.cell + offset;
            var pullFromCell = player.tile.cell - offset;

            // Move to the new cell immediately to ensure proper timing.  This will teleport the player
            // but the call to Tween.Move below will immediately set the player back to the previous position
            // to play the animation.
            player.tile.cell = _moveToCell;

            // Pull in the direction
            if (!player.SendToCell(new PullEvent(player.tile, offset, moveDuration), pullFromCell, CellEventRouting.FirstVisible))
            {
                // Move back since the pull failed
                player.tile.cell = _moveFromCell;
                return false;
            }

            GameManager.busy++;

            player.PlayAnimation("Push"); // FIXME: we need a pull

            // Animate the movement
            Tween.Move(player.puzzle.grid.CellToWorld(_moveFromCell), player.puzzle.grid.CellToWorld(_moveToCell), false)
                .Duration(moveDuration)
                //                .EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(player.tile.gameObject);

            return true;
        }

        private void UpdateVisuals(CellEdge edge)
        {
            _facingDirection = edge;

            switch (edge)
            {
                case CellEdge.North:
                    player.visuals.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;

                case CellEdge.South:
                    player.visuals.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    break;

                case CellEdge.East:
                    player.visuals.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    break;

                case CellEdge.West:
                    player.visuals.transform.localRotation = Quaternion.Euler(0, -90, 0);
                    break;
            }
        }
    }
}
