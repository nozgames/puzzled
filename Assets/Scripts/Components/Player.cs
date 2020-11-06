using UnityEngine;
using UnityEngine.InputSystem;
using NoZ;

namespace Puzzled
{
    class Player : PuzzledActorComponent
    {
        private Vector2Int moveToCell;
        private Animator animator;
        private Vector2Int queuedMove;
        private float queuedMoveTime = float.MinValue;

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

        private void OnLeftAction(InputAction.CallbackContext ctx) => MoveAsync(new Vector2Int(-1, 0));
        private void OnRightAction(InputAction.CallbackContext ctx) => MoveAsync(new Vector2Int(1, 0));
        private void OnUpAction(InputAction.CallbackContext ctx) => MoveAsync(new Vector2Int(0, 1));
        private void OnDownAction(InputAction.CallbackContext ctx) => MoveAsync(new Vector2Int(0, -1));

        private void MoveAsync (Vector2Int cell)
        {
            if (GameManager.IsBusy) 
            {
                queuedMove = cell;
                queuedMoveTime = Time.time;
                return;
            }

            queuedMoveTime = float.MinValue;
            cell += actor.Cell;

            if (cell.x < actor.Cell.x)
                visuals.localScale = new Vector3(-1, 1, 1);
            else if (cell.x > actor.Cell.x)
                visuals.localScale = Vector3.one;

            var query = ActorEvent.Singleton<QueryMoveEvent>().Init(cell);
            GameManager.Instance.SendToCell(query, query.Cell);
            if (!query.Result)
                return;

            BeginBusy();

            moveToCell = cell;

            PlayAnimation("Walk");

            Tween.Move(actor.transform.position, GameManager.CellToWorld(cell), false)
                .Duration(0.4f)
                .EaseOutCubic()
                .OnStop(OnMoveComplete)
                .Start(actor.gameObject);
        }

        private void OnMoveComplete()
        {
            SendToCell(ActorEvent.Singleton<LeaveCellEvent>().Init(), actor.Cell);
            GameManager.Instance.SetActorCell(actor, moveToCell);
            SendToCell(ActorEvent.Singleton<EnterCellEvent>().Init(), moveToCell);

            PlayAnimation("Idle");

            EndBusy();

            if (Time.time - queuedMoveTime <= queuedInputThreshold)
                MoveAsync(queuedMove);
        }

        private void PlayAnimation (string name)
        {
            animator.SetTrigger(name);
        }
    }
}

