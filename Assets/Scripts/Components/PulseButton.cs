using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PulseButton : TileComponent
    {
        [Editable]
        [Port(PortFlow.Output, PortType.Signal, legacy = true)]
        public Port usePort { get; set; }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            usePort.SendSignal();
        }
    }
}
