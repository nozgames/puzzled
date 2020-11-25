using System;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Theme", menuName = "Puzzled/Theme")]
    public class Theme : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct TilePrefab
        {
            public string id;
            public GameObject prefab;
        }

        [SerializeField] private TilePrefab[] prefabs;

        [NonSerialized] private Dictionary<TileType, GameObject> prefabsById = new Dictionary<TileType, GameObject>();

        public GameObject GetPrefab(TileType tileId) => prefabsById.TryGetValue(tileId, out var gameObject) ? gameObject : null;

        public void SetPrefab(TileType tileId, GameObject prefab) => prefabsById[tileId] = prefab;


        public void OnBeforeSerialize()
        {
            prefabs = new TilePrefab[prefabsById.Count];

            var i = 0;
            foreach(var entry in prefabsById)
                prefabs[i++] = new TilePrefab { id = entry.Key.ToString(), prefab = entry.Value };
        }

        public void OnAfterDeserialize()
        {
            prefabsById.Clear();

            if (prefabs == null)
                return;

            foreach (var prefab in prefabs)
                if (Enum.TryParse<TileType>(prefab.id, out var id))
                    prefabsById[id] = prefab.prefab;
        }
    }
}
