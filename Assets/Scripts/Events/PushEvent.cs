using UnityEngine;
using NoZ;

namespace Puzzled
{
    class PushEvent : ActorEvent
    {
        public Vector2Int Cell { get; private set; }

        public PushEvent Init(Vector2Int cell)
        {
            Cell = cell;
            return this;
        }
    }
}
