using NoZ;

namespace Puzzled
{
    class BeamChangedEvent : ActorEvent
    {
        public Beam beam { get; private set; }
        public bool isConnecting { get; private set; }
        public bool isDisconnecting => !isConnecting;

        public BeamChangedEvent(Beam beam, bool connecting)
        {
            this.beam = beam;
            this.isConnecting = connecting;
        }
    }
}
