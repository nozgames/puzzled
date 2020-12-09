using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

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

            var assets = new List<Tuple<Guid, IResourceLocation>>();
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

                    assets.Add(new Tuple<Guid, IResourceLocation>(keyGuid, keyLocations[0]));
                }
            }

            var loadOperation = Addressables.LoadAssetsAsync<TAsset>(assets.Select(t => t.Item2).ToList(), obj => { }, true);
            yield return loadOperation;

            for(int i=0; i<loadOperation.Result.Count; i++)
                _cache[assets[i].Item1] = loadOperation.Result[i];

            loaded = true;

            OnLoaded();
        }

        protected virtual void OnLoaded()
        {
        }
    }
}
