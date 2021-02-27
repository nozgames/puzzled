using NoZ;

namespace Puzzled
{
    class ShowGizmosEvent : ActorEvent
    {
        public bool show { get; private set; }

        public ShowGizmosEvent(bool show)
        {
            this.show = show;
        }
    }
}
