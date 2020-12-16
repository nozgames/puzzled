using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class DecalDatabase : AddressableDatabase<Sprite>
    {
        private static DecalDatabase _instance = null;

        private Decal[] _decals;
        private Dictionary<Guid, Decal> _decalsByGuid;

        private void Awake()
        {
            _instance = this;
        }

        public static Decal GetDecal(Guid guid) => _instance._decalsByGuid.TryGetValue(guid, out var decal) ? decal : null;

        protected override string label => "decal";

        protected override void OnLoaded()
        {
            _decals = _instance._cache.Select(kv => new Decal(kv.Key, kv.Value)).ToArray();
            _decalsByGuid = new Dictionary<Guid, Decal>();

            foreach (var decal in _decals)
                _decalsByGuid[decal.guid] = decal;
        }
    }
}

