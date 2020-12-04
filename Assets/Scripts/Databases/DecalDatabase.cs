using System;
using UnityEngine;

namespace Puzzled
{
    public class DecalDatabase : AddressableDatabase<Sprite>
    {
        private static DecalDatabase instance = null;

        private void Awake()
        {
            instance = this;
        }

        public static Sprite GetAudioClip(Guid guid) => instance.GetAsset(guid);

        protected override string label => "decal";
    }
}
