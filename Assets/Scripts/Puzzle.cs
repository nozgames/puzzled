using System;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class Puzzle : ScriptableObject
    {
        public GameObject puzzlePrefab;

        [Serializable]
        public class SerializedTile
        {
            public Vector2Int cell;
            public GameObject prefab;
        }

        [Serializable]
        private class Connection
        {
            public int from;
            public int to;
        }

        [SerializeField] [HideInInspector] private SerializedTile[] tiles;
        [SerializeField] [HideInInspector] private Connection[] wires;

        public SerializedTile[] tempTiles => tiles;

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
            var tiles = target.GetComponentsInChildren<Tile>();
            var save = new SerializedTile[tiles.Length];

            wires = new Connection[tiles.Sum(t => t.connections.Length)];
            var connectionIndex = 0;
            for(int i=0; i<tiles.Length; i++)
            {
                var tile = tiles[i];
                var editorInfo = tile.GetComponent<TileEditorInfo>();
                save[i] = new SerializedTile {
                    prefab = tile.GetComponent<TileEditorInfo>().prefab.gameObject,
                    cell = tile.cell
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

                this.tiles = save;
            }
        }
    }
}
