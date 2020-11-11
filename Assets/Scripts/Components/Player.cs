﻿using UnityEngine;
using UnityEngine.InputSystem;
using NoZ;

namespace Puzzled
{
    class Player : PuzzledActorComponent
    {
        private Vector2Int moveFromCell;
        private Vector2Int moveToCell;
        private Animator animator;
        private Vector2Int queuedAction;
        private float queuedActionTime = float.MinValue;

        [SerializeField] private float queuedInputThreshold = 0.25f;

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

        private bool Move (Vector2Int cell)
        {
            moveFromCell = actor.Cell;
            moveToCell = actor.Cell + cell;

            var query = new QueryMoveEvent(actor, cell);
            GameManager.Instance.SendToCell(query, moveToCell);
            if (!query.result)
                return false;

            BeginBusy();

            PlayAnimation("Walk");

            Tween.Move(actor.transform.position, GameManager.CellToWorld(moveToCell), false)
                .Duration(0.4f)
                .EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(actor.gameObject);

            return true;
        }

        private bool PushMove (Vector2Int cell)
        {
            moveFromCell = actor.Cell;
            moveToCell = actor.Cell + cell;

            var query = new QueryPushEvent(actor, cell);
            GameManager.Instance.SendToCell(query, moveToCell);
            if (!query.result)
                return false;

            BeginBusy();

            PlayAnimation("Push");

            GameManager.Instance.SendToCell(new PushEvent(actor, cell), moveToCell);

            Tween.Move(actor.transform.position, GameManager.CellToWorld(moveToCell), false)
                .Duration(0.4f)
                .EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(actor.gameObject);

            return true;
        }

        private bool UseMove (Vector2Int cell)
        {
            moveFromCell = actor.Cell;
            moveToCell = actor.Cell + cell;

            var query = new QueryUseEvent(actor, cell);
            GameManager.Instance.SendToCell(query, moveToCell);
            if (!query.result)
                return false;

            BeginBusy();

            PlayAnimation("Push");

            GameManager.Instance.SendToCell(new UseEvent(actor), moveToCell);

            return true;
        }

        private void OnMoveComplete()
        {
            SendToCell(ActorEvent.Singleton<LeaveCellEvent>().Init(), moveFromCell);
            GameManager.Instance.SetActorCell(actor, moveToCell);
            SendToCell(ActorEvent.Singleton<EnterCellEvent>().Init(), moveToCell);

            PlayAnimation("Idle");

            EndBusy();

            OnActionComplete();
        }

        private void PlayAnimation (string name)
        {
            animator.SetTrigger(name);
        }
    }
}

