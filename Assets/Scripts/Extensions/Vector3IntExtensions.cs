using UnityEngine;

namespace Puzzled
{
    static class Vector3IntExtensions
    {
        public static Vector2Int ToVector2Int(this Vector3Int v) => new Vector2Int(v.x, v.y);
    }
}
