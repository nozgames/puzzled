using System;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName="New Puzzle", menuName ="Puzzled/Puzzle")]
    public class Puzzle : ScriptableObject
    {
        public GameObject puzzlePrefab;

        [Serializable]
        private class SerializedTile
        {
            public Vector2Int cell;
            public string prefab;
        }

        [Serializable]
        private class Connection
        {
            public int from;
            public int to;
        }

        [SerializeField] [HideInInspector] private SerializedTile[] tiles;
        [SerializeField] [HideInInspector] private Connection[] wires;

        /// <summary>
        /// Load the puzzle into the given target
        /// </summary>
        /// <param name="target"></param>
        public bool LoadInto (Transform target)
        {
            if (null == tiles)
                return true;

            var result = true;
            foreach (var tile in tiles)
            {
/*                var prefab = theme.GetPrefab(tile.prefab);
                if(null == prefab)
                {
                    Debug.LogWarning($"missing prefab for tile '{tile.type}");
                    result = false;
                    continue;
                }
                Instantiate(prefab, target);
*/            }

            return result;
        }

        public void Save (Transform target)
        {
#if false
            var tiles = target.GetComponentsInChildren<SerializedTile>();
            var save = new SerializedTile[tiles.Length];

            wires = new Connection[tiles.Sum(t => t.connections.Length)];
            var connectionIndex = 0;
            for(int i=0; i<tiles.Length; i++)
            {
                var tile = tiles[i];
                save[i] = new SerializedTile {
                    prefab = "",
                    cell = tile.Cell
                };

                foreach(var connectedTile in tile.connections)
                {
                    var to = 0;
                    for (to = 0; to < tiles.Length && tiles[to] != connectedTile; to++);
                    wires[connectionIndex++] = new Connection {
                        from = i,
                        to = i
                    };
                }
            }
#endif
        }
    }
}
