using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Puzzled
{
    public class SoundDatabase : ResourceDatabase<AudioClip>
    {
#if UNITY_EDITOR
        public static SoundDatabase GetInstance() => GetInstance<SoundDatabase>();
#endif
    }
}
