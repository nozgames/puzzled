using NoZ;

namespace Puzzled
{
    class LogicXAnd : TileComponent
    {
        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnWirePowerChanged(WirePowerChangedEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateState();

        private void UpdateState()
        {
            var powered = true;
            for (int i = 0; i < powerInPort.wireCount && powered; ++i)
                powered &= (powerInPort.GetWire(i).hasPower == (powerInPort.GetWireOption(i, 0) == 1));

            powerOutPort.SetPowered(powered);
        }
    }
}
