using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Puzzled
{
    public class DecalDatabase : ResourceDatabase<Texture>
    {
#if UNITY_EDITOR
        public static DecalDatabase GetInstance() => GetInstance<DecalDatabase>();
#endif
    }
}
