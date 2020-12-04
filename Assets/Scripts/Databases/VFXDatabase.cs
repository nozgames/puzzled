using System;
using UnityEngine;

namespace Puzzled
{
    public class VFXDatabase : AddressableDatabase<GameObject>
    {
        private static VFXDatabase instance = null;

        private void Awake()
        {
            instance = this;
        }

        public static GameObject GetAudioClip(Guid guid) => instance.GetAsset(guid);

        protected override string label => "vfx";
    }
}
