using System;
using UnityEngine;

namespace Puzzled
{
    public class SFXDatabase : AddressableDatabase<AudioClip>
    {
        private static SFXDatabase instance = null;

        private void Awake()
        {
            instance = this;
        }

        public static AudioClip GetAudioClip(Guid guid) => instance.GetAsset(guid);

        protected override string label => "sfx";
    }
}
