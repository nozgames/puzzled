using NoZ;

namespace Puzzled
{
    public class Teleporter : TileComponent
    {
        private int _lastTick = int.MinValue;

        [Editable]
        [Port(PortFlow.Input, PortType.Signal)]
        private Port signalInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Signal)]
        private Port signalOutPort { get; set; }

        [ActorEventHandler]
        private void OnStartEvent (StartEvent evt)
        {
            _lastTick = int.MinValue;
        }

        [ActorEventHandler]
        private void OnSignal (SignalEvent evt)
        {
            if (isEditing)
                return;

            if (_lastTick == tile.tickFrame)
                return;

            if (puzzle.player.Teleport (tile.cell))
            {
                _lastTick = tile.tickFrame;
                puzzle.player.tile.cell = tile.cell;
                signalOutPort.SendSignal();
            }
        }
    }
}
