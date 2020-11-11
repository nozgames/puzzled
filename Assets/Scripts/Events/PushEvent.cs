using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class PushEvent : ActorEvent
    {
        public Actor source { get; private set; }
        public Vector2Int offset { get; private set; }

        public PushEvent (Actor source, Vector2Int offset)
        {
            this.source = source;
            this.offset = offset;
        }
    }
}
