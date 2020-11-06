using NoZ;
using Puzzled;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzled
{
    public class InputPort : ActorComponent
    {
        [SerializeField] private string portName;
        [SerializeField] private UnityEvent triggerAction;
        
        public void HandleTrigger()
        {
            triggerAction.Invoke();
        }
    }
}
