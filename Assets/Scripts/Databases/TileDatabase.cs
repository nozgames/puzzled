using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Puzzled
{
    public class TileDatabase : ResourceDatabase<GameObject>
    {
#if UNITY_EDITOR
        public static TileDatabase GetInstance() => GetInstance<TileDatabase>();
#endif
    }
}
