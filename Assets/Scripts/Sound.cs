using System;
using UnityEngine;

namespace Puzzled
{
    public struct Sound 
    {
        public static readonly Sound none = new Sound { guid = Guid.Empty, clip = null };

        public Guid guid;
        public AudioClip clip;
    }
}
