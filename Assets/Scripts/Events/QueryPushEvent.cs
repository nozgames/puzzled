using UnityEngine;
using NoZ;

namespace Puzzled
{
    class QueryPushEvent : ActorEvent
    {
        public Vector2Int Cell { get; private set; }

        private bool result = false;

        public bool Result {
            get => result;
            set {
                result = value;
                if (result)
                    IsHandled = true;
            }
        }

        public QueryPushEvent Init(Vector2Int cell)
        {
            Cell = cell;
            Result = false;
            return this;
        }
    }
}
