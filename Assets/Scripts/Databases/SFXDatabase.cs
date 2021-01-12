using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class SFXDatabase : AddressableDatabase<AudioClip>
    {
        private static SFXDatabase _instance = null;

        private Sound[] _sfx;
        private Dictionary<Guid, Sound> _sfxByGuid;

        private void Awake()
        {
            _instance = this;
        }

        public static bool isLoaded => _instance != null && _instance.loaded;

        public static Sound[] GetSounds() => _instance._sfxByGuid.Values.ToArray();

        public static Sound GetSound(Guid guid) => _instance._sfxByGuid.TryGetValue(guid, out var sound) ? sound : Sound.none;

        protected override string label => "sfx";

        protected override void OnLoaded()
        {
            _sfx = _instance._cache
                .Select(kv => new Sound { guid = kv.Key, clip = kv.Value })
                .OrderBy(s => s.clip.name)
                .ToArray();
            _sfxByGuid = new Dictionary<Guid, Sound>();

            foreach (var sfx in _sfx)
                _sfxByGuid[sfx.guid] = sfx;
        }
    }
}

