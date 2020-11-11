using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class QueryMoveEvent : QueryEvent
    {
        public QueryMoveEvent(Actor source, Vector2Int offset) : base(source, offset) { }
    }
}
