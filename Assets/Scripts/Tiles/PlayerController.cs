using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzled
{
    public abstract class PlayerController : MonoBehaviour
    {
        [SerializeField] protected InputActionReference useAction = null;

        private Player _player;
        protected Player player => _player;

        public virtual void HandleEnable(Player player)
        {
            _player = player;
        }

        public virtual void HandleDisable()
        {
            _player = null;
        }

        public abstract void PreMove();
        public abstract void Move();
        public abstract void HandleGrabStarted(GrabEvent evt);
        public abstract void UpdateRotation();
    }
}
