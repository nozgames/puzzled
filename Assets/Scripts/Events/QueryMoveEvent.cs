using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class QueryMoveEvent : QueryEvent
    {
        public QueryMoveEvent(PuzzledActor source, Vector2Int offset) : base(source, offset) { }
    }
}
