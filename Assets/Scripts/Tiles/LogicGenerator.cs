using NoZ;

namespace Puzzled
{
    class LogicGenerator : TileComponent
    {
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        private Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            powerOutPort.SetPowered(true);
        }
    }
}
