using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Puzzled
{
    public class ResourceDatabase<TAsset> : ScriptableObject, ISerializationCallbackReceiver where TAsset : UnityEngine.Object
    {
        [Serializable]
        private struct SerializedResource
        {
            public string guid;
            public TAsset asset;
        }

        public struct Resource
        {
            public Guid guid;
            public TAsset asset;
        }

        [SerializeField] private List<SerializedResource> _serialized = null;

        protected Dictionary<Guid, Resource> _resources = new Dictionary<Guid, Resource>();

#if UNITY_EDITOR
        private static ResourceDatabase<TAsset> _instance = null;

        protected static TDatabase GetInstance<TDatabase>() where TDatabase : ResourceDatabase<TAsset>
        {
            if (_instance != null)
                return (TDatabase)_instance;

            var name = typeof(TDatabase).Name;
            _instance = AssetDatabase.LoadAssetAtPath<ResourceDatabase<TAsset>>($"Assets/Databases/{name}.asset");
            if (null == _instance)
            {
                _instance = CreateInstance<TDatabase>();
                EditorUtility.SetDirty(_instance);
                if (!AssetDatabase.IsValidFolder("Assets/Databases"))
                    AssetDatabase.CreateFolder("Assets", "Databases");
                AssetDatabase.CreateAsset(_instance, $"Assets/Databases/{name}.asset");
            }

            return (TDatabase)_instance;
        }

        private Guid GetGuidFromAssetDatabase(TAsset asset) =>
            Guid.Parse(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(asset)).ToString());

        public bool Contains(TAsset asset) =>
            _resources.ContainsKey(GetGuidFromAssetDatabase(asset));

        public void Add(TAsset asset)
        {
            var guid = GetGuidFromAssetDatabase(asset);
            if (_resources.ContainsKey(guid))
                return;

            var resource = new Resource { asset = asset, guid = guid };
            _resources[guid] = resource;
            OnDeserializeResource(resource);

            _serialized.Add(new SerializedResource { asset = asset, guid = guid.ToString() }); 

            EditorUtility.SetDirty(this);
        }

        public void Remove(TAsset asset)
        {
            _resources.Remove(GetGuidFromAssetDatabase(asset));

            var serializedIndex = _serialized.FindIndex(m => m.asset == asset);
            if (serializedIndex != -1)
                _serialized.RemoveAt(serializedIndex);

            EditorUtility.SetDirty(this);
        }
#endif

        public Resource[] GetResources () => _resources.Values.ToArray();

        public Resource GetResource(Guid guid) => _resources.TryGetValue(guid, out var asset) ? asset : new Resource { asset = null, guid = Guid.Empty };

        public Guid GetGuid(TAsset asset)
        {
            foreach (var kv in _resources)
                if (kv.Value.asset == asset)
                    return kv.Key;

            return Guid.Empty;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            _resources = new Dictionary<Guid, Resource>(_serialized.Count);

            if (_serialized == null)
                return;

            foreach (var serializedResource in _serialized)
            {
                if (serializedResource.asset == null)
                {
                    Debug.LogWarning($"Asset missing for guid {serializedResource.guid}");
                    continue;
                }

                if (!Guid.TryParse(serializedResource.guid, out var guid))
                {
                    Debug.LogWarning($"Guid failed to parse {serializedResource.guid}");
                    continue;
                }                    

                var resource = new Resource { guid = guid, asset = serializedResource.asset };
                OnDeserializeResource(resource);
                _resources[guid] = resource;
            }
        }

        protected virtual void OnDeserializeResource(Resource resource) { }
    }
}
