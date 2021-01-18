using NoZ;

namespace Puzzled
{
    public class WirelessSignal : TileComponent
    {
        [Editable]
        private string signal { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal)]
        private Port sendSignalPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Signal)]
        private Port receiveSignalPort { get; set; }

        [ActorEventHandler]
        private void OnAwake(AwakeEvent evt)
        {
            puzzle.onWirelessSignal += OnReceiveSignal;
        }

        [ActorEventHandler]
        private void OnDestroyEvent(DestroyEvent evt)
        {
            puzzle.onWirelessSignal -= OnReceiveSignal;
        }

        [ActorEventHandler]
        private void OnSignal(SignalEvent evt)
        {
            puzzle.SendWirelessSignal(signal);
        }

        private void OnReceiveSignal(string incomingSignal)
        {
            if (string.Compare(incomingSignal, signal, false) == 0)
                receiveSignalPort.SendSignal();
        }
    }
}
