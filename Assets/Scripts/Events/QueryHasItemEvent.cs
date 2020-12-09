using System;
using NoZ;

namespace Puzzled
{
    class QueryHasItemEvent : ActorEvent
    {
        public Guid itemGuid { get; private set; }
        
        public QueryHasItemEvent(Guid itemGuid)
        {
            this.itemGuid = itemGuid;
        }
    }
}
