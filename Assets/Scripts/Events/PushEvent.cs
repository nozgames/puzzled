using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class PushEvent : ActorEvent
    {
        public PuzzledActor source { get; private set; }
        public Vector2Int offset { get; private set; }
        public float duration { get; private set; }

        public PushEvent (PuzzledActor source, Vector2Int offset, float duration)
        {
            this.source = source;
            this.offset = offset;
            this.duration = duration;
        }
    }
}
