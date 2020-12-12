using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class QueryMoveEvent : QueryEvent
    {
        public QueryMoveEvent(Tile source, Cell offset) : base(source, offset) { }
    }
}
