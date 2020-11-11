using UnityEngine;
using NoZ;

namespace Puzzled
{
    class QueryUseEvent : QueryEvent
    {
        public QueryUseEvent(PuzzledActor user, Vector2Int offset) : base (user, offset)
        {
        }
    }
}
