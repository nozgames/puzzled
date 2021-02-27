using UnityEngine;

namespace Puzzled
{
    public static class BoundsExtensions
    {
        public static bool ContainsXZ(this Bounds bounds, Vector3 p) => 
            p.x >= bounds.min.x && p.x <= bounds.max.x && p.z >= bounds.min.z && p.z <= bounds.max.z;
    }
}
