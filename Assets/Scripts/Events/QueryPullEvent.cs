using UnityEngine;
using NoZ;

namespace Puzzled
{
    class QueryPullEvent : QueryEvent
    {
        public QueryPullEvent(Tile source, Vector2Int offset) : base(source, offset) { }
    }
}
