using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Number : TileComponent
    {
        [Editable]
        public int value { get; set; } = 0;

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true)]
        private Port signalInPort { get; set; }
        
        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        private Port valueOutPort { get; set; }

        [ActorEventHandler]
        private void OnSignalEvent(SignalEvent evt) => valueOutPort.SendValue(value);
    }
}
