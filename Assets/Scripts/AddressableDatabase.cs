using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Puzzled
{
    public class AddressableDatabaseBase : MonoBehaviour
    {
        public bool loaded { get; protected set; }
    }

    public class AddressableDatabase<TAsset> : AddressableDatabaseBase where TAsset : class 
    {
        protected Dictionary<Guid, TAsset> _cache = new Dictionary<Guid, TAsset>();

        protected virtual string label => "default";

        private void Start()
        {
            StartCoroutine(Initialize());
        }

        public TAsset GetAsset (Guid guid) => _cache.TryGetValue(guid, out var asset) ? asset : null;

        public Guid GetGuid (TAsset asset)
        {
            foreach (var kv in _cache)
                if (kv.Value == asset)
                    return kv.Key;

            return Guid.Empty;
        }

        public IEnumerator Initialize()
        {
            // Loading locations by label
            var labelOperation = Addressables.LoadResourceLocationsAsync(label);
            yield return labelOperation;

            var locationsForLabel = new HashSet<string>();
            foreach(var location in labelOperation.Result)
                locationsForLabel.Add(location.InternalId);

            foreach (var resourceLocator in Addressables.ResourceLocators)
            {
                foreach (var objKey in resourceLocator.Keys)
                {
                    if (!(objKey is string key))
                        continue;

                    if (!Guid.TryParse(key, out Guid keyGuid))
                        continue;

                    var hasLocation = resourceLocator.Locate(key, typeof(UnityEngine.Object), out var keyLocations);
                    if (!hasLocation)
                        continue;

                    var internalId = keyLocations[0].InternalId;
                    if (!locationsForLabel.Contains(internalId))
                        continue;

                    var loadOperation = Addressables.LoadAssetAsync<object>(internalId);
                    yield return loadOperation;

                    _cache[keyGuid] = (TAsset)loadOperation.Result;
                }
            }

            loaded = true;
        }
    }
}
