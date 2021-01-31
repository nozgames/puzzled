using UnityEngine;

namespace Puzzled
{
    static class Vector3Extensions
    {
        public static Vector3 ToXZ (this Vector3 v) => new Vector3(v.x, 0.0f, v.z);
    }
}
