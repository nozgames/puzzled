using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class RayCastEvent : ActorEvent
    {
        public Vector2Int direction { get; set; }
        public float offset { get; set; }

        private Tile _hit = null;

        public Tile hit {
            get => _hit;
            set {
                if (_hit != null)
                    return;
                _hit = value;
            }
        }

        public RayCastEvent (Vector2Int dir)
        {
            direction = dir;
        }
    }
}
