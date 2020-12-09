using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class PullEvent : ActorEvent
    {
        public Tile source { get; private set; }
        public Vector2Int offset { get; private set; }
        public float duration { get; private set; }

        public PullEvent(Tile source, Vector2Int offset, float duration)
        {
            this.source = source;
            this.offset = offset;
            this.duration = duration;
        }
    }
}
