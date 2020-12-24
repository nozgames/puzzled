using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class DecalDatabase : AddressableDatabase<Texture2D>
    {
        private static DecalDatabase _instance = null;

        private Decal[] _decals;
        private Dictionary<Guid, Decal> _decalsByGuid;

        private void Awake()
        {
            _instance = this;
        }

        public static bool isLoaded => _instance != null && _instance.loaded;

        public static Decal[] GetDecals() => _instance._decalsByGuid.Values.ToArray();

        public static Decal GetDecal(Guid guid) => _instance._decalsByGuid.TryGetValue(guid, out var decal) ? decal : null;

        protected override string label => "decal";

        protected override void OnLoaded()
        {
            _decals = _instance._cache
                .Select(kv => {
                    var decal = new Decal(kv.Key, Sprite.Create(kv.Value, new Rect(0, 0, kv.Value.width, kv.Value.height), new Vector2(.5f, .5f), 64));
                    decal.sprite.name = kv.Value.name.StartsWith("Decal") ? kv.Value.name.Substring(5) : kv.Value.name;
                    return decal;
                })
                .OrderBy(d => d.sprite.name)
                .ToArray();
            _decalsByGuid = new Dictionary<Guid, Decal>();

            foreach (var decal in _decals)
                _decalsByGuid[decal.guid] = decal;
        }
    }
}

