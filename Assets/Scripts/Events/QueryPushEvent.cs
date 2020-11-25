using UnityEngine;
using NoZ;

namespace Puzzled
{
    class QueryPushEvent : QueryEvent
    {
        public QueryPushEvent(Tile source, Vector2Int offset) : base(source, offset) { }
    }
}
