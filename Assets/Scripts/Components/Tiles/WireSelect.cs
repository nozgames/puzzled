using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Select))]
    class WireSelect : TileComponent
    {
        /// <summary>
        /// Output power port that poweres the wire matching the selectPort value
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnSelectUpdate(SelectUpdateEvent evt)
        {
            // Enable power for the selected wire and disabled for any other
            var wireIndex = evt.value - 1;
            for (int i = 0; i < powerOutPort.wireCount; ++i)
                powerOutPort.SetPowered(i, (i == wireIndex));
        }
    }
}
