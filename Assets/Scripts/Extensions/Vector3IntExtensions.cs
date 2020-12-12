using UnityEngine;

namespace Puzzled
{
    static class Vector3IntExtensions
    {
        public static Vector2Int ToVector2Int(this Vector3Int v) => new Vector2Int(v.x, v.y);

        public static Cell ToCell (this Vector3Int v) => new Cell(v.x, v.y);
    }
}
