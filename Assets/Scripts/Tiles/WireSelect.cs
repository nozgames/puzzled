using NoZ;
using UnityEngine;
using System.Collections.Generic;

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
            HashSet<int> wireValues = new HashSet<int>();

            // add transient value first (may be 0)
            if (evt.transientValue > 0)
                wireValues.Add(evt.transientValue - 1);

            foreach (Wire wire in evt.wires)
                wireValues.Add(wire.value - 1);

            // Enable power for the selected wires and disabled for any other
            for (int i = 0; i < powerOutPort.wireCount; ++i)
            {
                bool isPowered = wireValues.Contains(i) ;
                powerOutPort.SetPowered(i, isPowered);
            }
        }
    }
}
