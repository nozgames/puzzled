using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicAnd : TileComponent
    {
        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnWirePower(WirePowerChangedEvent evt) => UpdateState();

        private void UpdateState()
        {
            var powered = true;
            for (int i = 0; powered && i < powerInPort.wireCount; i++)
                powered = powered & powerInPort.GetWire(i).isPowered;

            powerOutPort.SetPowered(powered);
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateState();
    }
}
