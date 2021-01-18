using System.Collections.Generic;
using NoZ;

namespace Puzzled
{
    public class WirelessSignal : TileComponent
    {
        private class SharedData
        {
            public List<WirelessSignal> signals = new List<WirelessSignal>();
        }

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
            var shared = GetSharedData<SharedData>();
            if(null == shared)
            {
                shared = new SharedData();
                SetSharedData(shared);
            }

            shared.signals.Add(this);
        }

        [ActorEventHandler]
        private void OnDestroyEvent(DestroyEvent evt)
        {
            GetSharedData<SharedData>().signals.Remove(this);
        }

        [ActorEventHandler]
        private void OnSignal(SignalEvent evt)
        {
            foreach (var target in GetSharedData<SharedData>().signals)
            {
                if (string.Compare(signal, target.signal, true) == 0)
                    receiveSignalPort.SendSignal();
            }
        }
    }
}
