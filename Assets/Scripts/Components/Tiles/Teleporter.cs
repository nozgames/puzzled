using NoZ;

namespace Puzzled
{
    public class Teleporter : TileComponent
    {
        [Editable]
        [Port(PortFlow.Input, PortType.Signal)]
        private Port signalInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Signal)]
        private Port signalOutPort { get; set; }

        [ActorEventHandler]
        private void OnSignal (SignalEvent evt)
        {
            if (isEditing)
                return;

            if (puzzle.player.Teleport (tile.cell))
            {
                puzzle.player.tile.cell = tile.cell;
                signalOutPort.SendSignal();
            }
        }
    }
}
