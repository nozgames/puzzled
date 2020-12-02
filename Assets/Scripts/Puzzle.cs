using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class Puzzle 
    {
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

        public SerializedTile[] tiles;
        public SerializedWire[] wires;

        public static void Save (Transform target)
        {
            var puzzle = new Puzzle();
            var tiles = target.GetComponentsInChildren<Tile>();
            var save = new SerializedTile[tiles.Length];

            puzzle.wires = new SerializedWire[tiles.Sum(t => t.outputCount)];
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
                    puzzle.wires[wireIndex++] = new SerializedWire {
                        from = i,
                        to = to
                    };
                }

                puzzle.tiles = save;
            }
        }

        public static void Load(string filename, Action<Tile> tileCallback = null)
        {
            var puzzle = JsonUtility.FromJson<Puzzle>(File.ReadAllText(filename));

            GameManager.Instance.ClearTiles();

            if (puzzle.tiles == null)
                return;

            var tiles = new List<Tile>();
            foreach (var serializedTile in puzzle.tiles)
            {
                var tile = GameManager.InstantiateTile(serializedTile.prefab, serializedTile.cell);
                tiles.Add(tile);

                tileCallback?.Invoke(tile);
            }

            if (puzzle.wires != null)
                foreach (var serializedWire in puzzle.wires)
                    GameManager.InstantiateWire(tiles[serializedWire.from], tiles[serializedWire.to]);

            for (int i = 0; i < puzzle.tiles.Length; i++)
            {
                var serializedTile = puzzle.tiles[i];
                if (serializedTile.properties == null)
                    continue;

                var tileEditorInfo = tiles[i].GetComponent<TileEditorInfo>();
                foreach (var serializedProperty in serializedTile.properties)
                    tileEditorInfo.SetEditableProperty(serializedProperty.name, serializedProperty.value);
            }

            File.WriteAllText(filename, JsonUtility.ToJson(puzzle));
        }
    }
}
