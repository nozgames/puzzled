using NoZ;
using Puzzled;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class PressurePlate : ActorComponent
    {
        [SerializeField] private OutputPort activatePort;
        [SerializeField] private OutputPort deactivatePort;
        [SerializeField] private bool isSticky = false;

        [ActorEventHandler]
        private void OnEnter(EnterCellEvent evt)
        {
            activatePort.FireTrigger();
        }

        [ActorEventHandler]
        private void OnExit(LeaveCellEvent evt)
        {
            if (!isSticky)
                deactivatePort.FireTrigger();
        }
    }
}
