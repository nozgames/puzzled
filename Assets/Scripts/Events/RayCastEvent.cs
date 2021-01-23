﻿using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class RayCastEvent : ActorEvent
    {
        public Cell direction { get; set; }
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

        public RayCastEvent (Cell dir)
        {
            direction = dir;
        }
    }
}
