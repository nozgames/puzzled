using NoZ;

namespace Puzzled
{
    class LogicOr : TileComponent
    {
        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnWirePower(WirePowerChangedEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateState();

        private void UpdateState() => powerOutPort.SetPowered(powerInPort.hasPower);
    }
}
