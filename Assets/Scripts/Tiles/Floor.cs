using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Floor : TileComponent
    {
        [SerializeField] private AudioClip _footstepClip = null;

        public AudioClip footstepClip => _footstepClip;
    }
}
