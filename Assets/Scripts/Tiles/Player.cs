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
        private const float kTooltipDelay = 0.15f;

        [SerializeField] private PlayerController _playerController;

#if false
        private string _tooltip;
        private Cell _tooltipCell = Cell.invalid;
        private float _tooltipElapsed;
#endif

        public Tile inventory { get; private set; }

        [SerializeField] private GameObject _visuals = null;
        public GameObject visuals => _visuals;

        [SerializeField] private Animator _animator = null;
        public Animator animator => _animator;

        private int _rotationIndex = 0;
        private int numRotations = 4;

        [Editable(hidden = true)]
        public int rotation
        {
            get => _rotationIndex;
            set
            {
               this._rotationIndex = value % numRotations;
               _playerController.UpdateRotation();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _playerController.HandleEnable(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            var cell = tile.cell;
            if (cell != Cell.invalid)
                SendToCell(new LeaveCellEvent(tile, cell), cell);

            _playerController.HandleDisable();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            SendToCell(new EnterCellEvent(tile, tile.cell), tile.cell);
            _playerController.UpdateRotation();
        }

        [ActorEventHandler]
        private void OnDestroyEvent(DestroyEvent evt)
        {
            UIManager.HideTooltip();
        }

        [ActorEventHandler]
        private void OnUpdate(NoZ.ActorUpdateEvent evt)
        {
            _playerController.PreMove();

            if (GameManager.isBusy)
                return;

            UpdateTooltip();

            // handle move
            _playerController.Move();
        }

        [ActorEventHandler]
        private void OnGrab(GrabEvent evt)
        {
            Debug.Assert(evt.target.IsAdjacentTo(tile.cell));

            _playerController.HandleGrabStarted(evt);

            UIManager.HideTooltip();
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
                if (_tooltipElapsed < kTooltipDelay)
                {
                    _tooltipElapsed += Time.deltaTime;
                    if (_tooltipElapsed >= kTooltipDelay)
                    {
                        _tooltipElapsed = kTooltipDelay;
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

        public void PlayAnimation (string name)
        {
            //animator.SetTrigger(name);
        }

        [ActorEventHandler]
        private void OnGiveItem (GiveItemEvent evt)
        {
            var drop = inventory;
            if(drop != null)
            {
                // Don't bother picking up the same item
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
                    
                    static void Done ()
                    {
                        GameManager.Stop();
                        GameManager.UnloadPuzzle();
                        UIManager.ReturnToPlayWorldScreen();
                    }

                    var worldTransition = puzzle.world.transitionOut;
                    var transition = puzzle.entry.transitionOut;
                    if (transition != null)
                        UIManager.ShowWorldTransitionScreen(transition, () => {
                            if (worldTransition != null && puzzle.world.isCompleted)
                                UIManager.ShowWorldTransitionScreen(worldTransition, Done);
                            else
                                Done();
                        });
                    else if (worldTransition != null && puzzle.world.isCompleted)
                        UIManager.ShowWorldTransitionScreen(worldTransition, Done);
                    else
                        Done();
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
    }
}

