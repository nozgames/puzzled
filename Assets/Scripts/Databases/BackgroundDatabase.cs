
#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace Puzzled
{
    public class BackgroundDatabase : ResourceDatabase<Background>
    {
#if UNITY_EDITOR
        public static BackgroundDatabase GetInstance() => GetInstance<BackgroundDatabase>();
#endif

        protected override void OnDeserializeResource(Resource resource)
        {
            resource.asset.guid = resource.guid;
        }
    }
}
