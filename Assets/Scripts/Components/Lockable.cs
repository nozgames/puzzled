using NoZ;
using Puzzled;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class Lockable : ActorComponent
    {
//        [SerializeField] private bool isLocked = true;
        public bool isLocked = true;

        public void Lock()
        {
            isLocked = true;
        }

        public void Unlock()
        {
            isLocked = false;
        }
    }
}
