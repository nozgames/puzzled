using NoZ;
using UnityEngine;
using System.Collections.Generic;

namespace Puzzled
{
    class WireNumber : TileComponent
    {
        private int _value = 1;

        /// <summary>
        /// Input signal
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(TriggerSignal))]
        public Port signalPort { get; set; }

        /// <summary>
        /// Output value based on last signal
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        public Port valueOutPort { get; set; }

        [ActorEventHandler]
        private void OnSignal(TriggerSignal evt)
        {
            _value = signalPort.wires.IndexOf(evt.wire) + 1;

            valueOutPort.SendValue(_value);
        }
    }
}
