using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicSpinner : TileComponent
    {
        private int _value = 0;

        [Editable]
        public int valueCount { get; private set; }

        // TODO: minvalue 
        // TODO: maxvalue

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(IncrementEvent))]
        public Port incrementPort { get; set; }
        
        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        public Port valuePort { get; set; }

        [ActorEventHandler]
        private void OnIncrement (IncrementEvent evt)
        {
            _value = (_value + 1) % valueCount;
            SendValue();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => SendValue();

        private void SendValue() => valuePort.SendValue(_value + 1);
    }
}
