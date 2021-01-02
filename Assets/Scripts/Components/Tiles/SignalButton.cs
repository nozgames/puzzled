using NoZ;
using UnityEngine;

namespace Puzzled
{
    class SignalButton : TileComponent
    {
        [Editable]
        [Port(PortFlow.Output, PortType.Signal, legacy = true)]
        private Port signalOutPort { get; set; }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            signalOutPort.SendSignal();
        }
    }
}
