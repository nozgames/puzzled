using UnityEngine;

namespace Puzzled
{
    static class Vector2IntExtensions
    {
        public static Vector3Int ToVector3Int(this Vector2Int v) => new Vector3Int(v.x, v.y, 0);
    }
}
