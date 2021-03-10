using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Select))]
    class StateSelect : TileComponent
    {
        [Editable(hidden = true)]
        public string[] steps { get; set; }

        [Editable]
        public bool alwaysSignal { get; set; } = false;

        /// <summary>
        /// Output used to forward power to the current state
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        private Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnSelectUpdate(SelectUpdateEvent evt)
        {
            for (int i = 0; i < powerOutPort.wireCount; ++i)
            {
                int wireStates = powerOutPort.GetWireOption(i, 0);

                bool isPowered = false;

                // check transient value first
                if (evt.transientValue >= 0)
                {
                    int stateIndex = evt.transientValue;
                    if ((wireStates & (1 << stateIndex)) != 0)
                        isPowered = true;
                }

                foreach (Wire wire in evt.wires)
                {
                    int stateIndex = wire.value;
                    if ((wireStates & (1 << stateIndex)) != 0)
                        isPowered = true;
                }

                // for signal ports, toggle power if configured to always signal
                if ((powerOutPort.GetWire(i).to.port.type == PortType.Signal) && isPowered && alwaysSignal)
                    powerOutPort.SetPowered(i, false);

                powerOutPort.SetPowered(i, isPowered);
            }
        }
    }
}
