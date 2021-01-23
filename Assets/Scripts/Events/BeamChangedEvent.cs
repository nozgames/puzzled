using NoZ;

namespace Puzzled
{
    class BeamChangedEvent : ActorEvent
    {
        public Beam beam { get; private set; }

        public BeamChangedEvent(Beam beam)
        {
            this.beam = beam;
        }
    }
}
