using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class PushEvent : ActorEvent
    {
        public Tile source { get; private set; }
        public Vector2Int direction { get; private set; }
        public float duration { get; private set; }

        public PushEvent (Tile source, Vector2Int offset, float duration)
        {
            this.source = source;
            this.direction = offset;
            this.duration = duration;
        }
    }
}
