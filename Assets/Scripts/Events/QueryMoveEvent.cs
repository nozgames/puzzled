using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class QueryMoveEvent : ActorEvent
    {
        public Vector2Int Cell { get; private set; }

        private bool result = true;

        public bool Result {
            get => result;
            set {
                result = value;
                if (!result)
                    IsHandled = true;
            }
        }

        public QueryMoveEvent Init (Vector2Int cell)
        {
            Cell = cell;
            Result = true;
            return this;
        }
    }
}
