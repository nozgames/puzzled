using NoZ;
using Puzzled;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzled
{
    public class Port : ActorComponent
    {
        [SerializeField] private Port[] triggerPorts;

        [SerializeField] private UnityEvent activateAction;
        [SerializeField] private UnityEvent deactivateAction;

        public void Activate()
        {
            activateAction.Invoke();
        }

        public void Deactivate()
        {
            deactivateAction.Invoke();
        }

        public void FireActivate()
        {
            foreach (Port triggerPort in triggerPorts)
                triggerPort.Activate();
        }

        public void FireDeactivate()
        {
            foreach (Port triggerPort in triggerPorts)
                triggerPort.Deactivate();
        }

        public void FirePulse()
        {
            foreach (Port triggerPort in triggerPorts)
            {
                triggerPort.Activate();
                triggerPort.Deactivate();
            }
        }
    }
}
