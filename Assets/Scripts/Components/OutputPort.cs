using NoZ;
using Puzzled;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class OutputPort : ActorComponent
    {
        [SerializeField] private string portName;
        [SerializeField] private InputPort[] triggerPorts;

        public void FireTrigger()
        {
            foreach (InputPort triggerPort in triggerPorts)
                triggerPort.HandleTrigger();
        }
    }
}
