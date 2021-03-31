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
            // Enable power for the selected wires and disabled for any other
            for (int i = 0; i < powerOutPort.wireCount; ++i)
            {
                bool isWirePowered = false;
                if (evt.isPowered)
                {
                    isWirePowered = (i == evt.transientValue);
                    if (!isWirePowered)
                    {
                        // FIXME: is there a nicer way than iterating over each wire?  (the wire count should be pretty small in most cases)
                        foreach (Wire wire in evt.wires)
                        {
                            if (wire.value == i)
                            {
                                isWirePowered = true;
                                break;
                            }
                        }
                    }
                }

                powerOutPort.SetPowered(i, isWirePowered);
            }
        }
    }
}
