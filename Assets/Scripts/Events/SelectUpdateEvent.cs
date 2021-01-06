using NoZ;
using System.Collections.Generic;

namespace Puzzled
{
    public class SelectUpdateEvent : ActorEvent
    {
        public Select source { get; private set; }
        public List<Wire> wires { get; private set; }
        public int transientValue { get; private set; }

        public SelectUpdateEvent(Select source, int transientValue, List<Wire> wires)
        {
            this.source = source;
            this.wires = wires;
            this.transientValue = transientValue;
        }
    }
}
