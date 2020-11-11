using UnityEngine;
using NoZ;

namespace Puzzled
{
    class QueryPushEvent : QueryEvent
    {
        public QueryPushEvent(Actor source, Vector2Int offset) : base(source, offset) { }
    }
}
