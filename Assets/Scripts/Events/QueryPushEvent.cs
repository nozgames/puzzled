using UnityEngine;
using NoZ;

namespace Puzzled
{
    class QueryPushEvent : QueryEvent
    {
        public QueryPushEvent(PuzzledActor source, Vector2Int offset) : base(source, offset) { }
    }
}
