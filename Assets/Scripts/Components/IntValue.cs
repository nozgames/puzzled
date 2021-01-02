using NoZ;
using UnityEngine;

namespace Puzzled
{
    class IntValue : TileComponent
    {
        [Editable]
        public int value { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true)]
        public Port triggerPort { get; set; }
        
        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        public Port valuePort { get; set; }

        [ActorEventHandler]
        private void OnSignalEvent(SignalEvent evt) => valuePort.SendValue(value);
    }
}
