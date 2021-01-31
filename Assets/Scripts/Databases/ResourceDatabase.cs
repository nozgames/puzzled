﻿using System;
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

        [SerializeField] private SerializedResource[] _serializedResources = null;

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

            EditorUtility.SetDirty(this);
        }

        public void Remove(TAsset asset)
        {
            _resources.Remove(GetGuidFromAssetDatabase(asset));
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

        public void OnBeforeSerialize()
        {
            _serializedResources = _resources.Select(e => new SerializedResource { asset = e.Value.asset, guid = e.Key.ToString() }).ToArray();
        }

        public void OnAfterDeserialize()
        {
            if (_serializedResources != null)
            {
                foreach (var serializedResource in _serializedResources)
                {
                    if (serializedResource.asset == null)
                        continue;

                    if (!Guid.TryParse(serializedResource.guid, out var guid))
                        continue;

                    var resource = new Resource { guid = guid, asset = serializedResource.asset };
                    OnDeserializeResource(resource);
                    _resources[guid] = resource;
                }

                _serializedResources = null;
            }
        }

        protected virtual void OnDeserializeResource(Resource resource) { }
    }
}
