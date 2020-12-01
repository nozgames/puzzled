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
            public string prefab;
            public SerializedProperty[] properties;
        }

        [Serializable]
        public class SerializedWire
        {
            public int from;
            public int to;
        }

        [Serializable]
        public class SerializedProperty
        {
            public string name;
            public string value;
        }

        [SerializeField] [HideInInspector] private SerializedTile[] tiles;
        [SerializeField] [HideInInspector] private SerializedWire[] wires;

        public SerializedTile[] tempTiles => tiles;
        public SerializedWire[] tempWires => wires;

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

            wires = new SerializedWire[tiles.Sum(t => t.outputCount)];
            var wireIndex = 0;
            for(int i=0; i<tiles.Length; i++)
            {
                var tile = tiles[i];
                var editorInfo = tile.GetComponent<TileEditorInfo>();
                save[i] = new SerializedTile {
                    prefab = tile.guid,
                    cell = tile.cell,
                    properties = editorInfo.editableProperties?
                        .Select(p => new SerializedProperty { name = p.property.Name, value = p.GetValue() })
                        .ToArray()
                };

                foreach(var output in tile.outputs)
                {
                    var to = 0;
                    for (to = 0; to < tiles.Length && tiles[to] != output.output; to++);
                    wires[wireIndex++] = new SerializedWire {
                        from = i,
                        to = to
                    };
                }

                this.tiles = save;
            }
        }
    }
}
