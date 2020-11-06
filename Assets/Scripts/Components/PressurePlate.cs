using NoZ;
using Puzzled;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class PressurePlate : ActorComponent
    {
        [SerializeField] private bool isSticky = false;

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            // do stuff
        }
    }
}
