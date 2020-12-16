using System;
using UnityEngine;

namespace Puzzled
{
    public class Decal
    {
        public Guid guid { get; private set; }

        public Sprite sprite { get; private set; }

        public Decal(Guid guid, Sprite sprite)
        {
            this.guid = guid;
            this.sprite = sprite;
        }
    }
}
