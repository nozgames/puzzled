using UnityEngine;
using NoZ;
using UnityEngine.InputSystem;
using System;

namespace Puzzled
{
    class MoveOnInput : PuzzledActorComponent
    {
        [SerializeField] private InputActionReference inputAction = null;
        [SerializeField] private Vector2Int offset;

        private void Start()
        {
            inputAction.action.Enable();
            inputAction.action.performed += OnAction;
        }

        private void OnAction(InputAction.CallbackContext ctx)
        {
            var query = ActorEvent.Singleton<QueryMoveEvent>().Init(actor.Cell + offset);
            GameManager.Instance.SendToCell(query, query.Cell);
            if(query.Result)
                actor.Cell += offset;

            // TODO: send failed move event here if cant move?
        }
    }
}
