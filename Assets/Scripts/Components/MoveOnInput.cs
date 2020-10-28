using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzled
{
    class MoveOnInput : PuzzledActorComponent
    {
        [SerializeField] private InputActionReference leftAction = null;
        [SerializeField] private InputActionReference rightAction = null;
        [SerializeField] private InputActionReference upAction = null;
        [SerializeField] private InputActionReference downAction = null;

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

        private void OnLeftAction(InputAction.CallbackContext ctx) 
        {
            if (actor.QueryMoveLeft())
                actor.MoveLeftAsync();
        }

        private void OnRightAction(InputAction.CallbackContext ctx) 
        {
            if (actor.QueryMoveRight())
                actor.MoveRightAsync();
        }

        private void OnUpAction(InputAction.CallbackContext ctx) 
        {
            if (actor.QueryMoveUp())
                actor.MoveUpAsync();
        }

        private void OnDownAction(InputAction.CallbackContext ctx) 
        {
            if (actor.QueryMoveDown())
                actor.MoveDownAsync();
        }
    }
}
