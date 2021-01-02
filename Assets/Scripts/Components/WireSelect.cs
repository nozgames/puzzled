using NoZ;
using UnityEngine;

namespace Puzzled
{
    class WireSelect : TileComponent
    {
        /// <summary>
        /// Selected wire index
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Number, legacy = true)]
        public Port selectPort { get; set; }

        [ActorEventHandler]
        private void OnValueSignal(ValueSignalEvent evt) => UpdateOutputs(evt.value);

        private void UpdateOutputs(int value)
        {
            // Signal all number ouputs with the new index
            tile.SignalOutputs(value);

            // Enable power for the selected wire and disabled for any other
            var wireIndex = value - 1;
            for (int i = 0; i < tile.outputCount; ++i)
                tile.SetOutputPowered(i, (i == wireIndex));
        }
    }
}
