using NoZ;
using UnityEngine;

namespace Puzzled
{
    class PulseButton : TileComponent
    {
        [Editable]
        [Port(PortFlow.Output, PortType.Signal, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            powerOutPort.SendSignal();
        }
    }
}
