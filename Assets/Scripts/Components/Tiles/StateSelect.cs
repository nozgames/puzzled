using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Select))]
    class StateSelect : TileComponent
    {
        [Editable(hidden = true)]
        public string[] steps { get; set; }

        /// <summary>
        /// Output used to forward power to the current state
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        private Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnSelectUpdate(SelectUpdateEvent evt)
        {
            var stateIndex = evt.value - 1;
            for (int i = 0; i < powerOutPort.wireCount; ++i)
                powerOutPort.SetPowered(i, (powerOutPort.GetWireOption(i, 0) & (1 << stateIndex)) != 0);
        }
    }
}
