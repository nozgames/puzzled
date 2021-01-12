using NoZ;

namespace Puzzled
{
    public class SoundFX : TileComponent
    {
        [Editable]
        private Sound sfx { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true)]
        private Port signalInPort { get; set; }

        [ActorEventHandler]
        private void OnSignal (SignalEvent evt)
        {
            PlaySound(sfx.clip);
        }
    }
}
