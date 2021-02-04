using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class FollowCamera : GameCamera
    {
        public override Vector3 target => puzzle.player != null ? puzzle.player.transform.position + new Vector3(offset.x * 0.25f, offset.y * 0.25f) : base.target;
    }
}
