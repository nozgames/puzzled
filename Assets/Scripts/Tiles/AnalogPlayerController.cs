using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzled
{
    public class AnalogPlayerController : PlayerController
    {
        [SerializeField] private InputActionReference movementAction = null;
        [SerializeField] private float movementSpeed = 2.0f;

        private Vector2 movementDir;
        private Vector2 lastMovementDir = Vector2.zero;

        public override void HandleEnable(Player player)
        {
            base.HandleEnable(player);

            movementAction.action.Enable();
            movementAction.action.performed += OnMovementPerformed;
            movementAction.action.canceled += OnMovementCanceled;
        }

        public override void HandleDisable()
        {
            base.HandleDisable();

            movementAction.action.performed -= OnMovementPerformed;
            movementAction.action.canceled -= OnMovementCanceled;
            movementAction.action.Disable();
        }

        public override void PreMove()
        {
        }

        public override void Move()
        {
            Vector2 moveOffset = movementDir * Time.deltaTime * movementSpeed;
            player.tile.transform.position = new Vector3(player.tile.transform.position.x + moveOffset.x, player.tile.transform.position.y, player.tile.transform.position.z + moveOffset.y);

            bool isMoving = false;
            if (movementDir.magnitude > 0.01)
            {
                lastMovementDir = movementDir;
                isMoving = true;
            }

            float angle = Mathf.Atan2(lastMovementDir.y, lastMovementDir.x);

            player.visuals.transform.localRotation = Quaternion.Euler(0, 360 - (Mathf.Rad2Deg * angle - 90), 0);

            player.animator.SetBool("Walking", isMoving);
        }

        public override void HandleGrabStarted(GrabEvent evt)
        {

        }

        public override void UpdateRotation()
        {

        }

        private void OnMovementPerformed(InputAction.CallbackContext ctx)
        {
            movementDir = ctx.ReadValue<Vector2>();
        }

        private void OnMovementCanceled(InputAction.CallbackContext ctx)
        {
            movementDir = Vector2.zero;
        }
    }
}
