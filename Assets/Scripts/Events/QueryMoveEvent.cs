using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class QueryMoveEvent : QueryEvent
    {
        public QueryMoveEvent(Tile source, Vector2Int offset) : base(source, offset) { }
    }
}
