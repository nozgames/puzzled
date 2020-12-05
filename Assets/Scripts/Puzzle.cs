using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class Puzzle 
    {
        public string name { get; set; }

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
            public int fromOrder;
            public int[] fromOptions;

            public int to;
            public int toOrder;
            public int[] toOptions;
        }

        [Serializable]
        public class SerializedProperty
        {
            public string name;
            public string value;
        }

        public SerializedTile[] tiles;
        public SerializedWire[] wires;

        public static void Save (Transform target, string filename)
        {
            File.WriteAllText(filename, JsonUtility.ToJson(Save(target)));
        }

        public static Puzzle Save (Transform target)
        {
            var puzzle = new Puzzle();
            var tiles = target.GetComponentsInChildren<Tile>();
            var save = new SerializedTile[tiles.Length];

            puzzle.wires = new SerializedWire[tiles.Sum(t => t.outputCount)];
            var wireIndex = 0;
            for(int tileIndex=0; tileIndex<tiles.Length; tileIndex++)
            {
                var tile = tiles[tileIndex];
                var editorInfo = tile.GetComponent<TileEditorInfo>();
                save[tileIndex] = new SerializedTile {
                    prefab = editorInfo.guid.ToString(),
                    cell = tile.cell,
                    properties = editorInfo.editableProperties?
                        .Select(p => new SerializedProperty { name = p.property.Name, value = p.GetValue() })
                        .ToArray()
                };

                for(int outputIndex=0; outputIndex<tile.outputs.Count; outputIndex++)
                {
                    var output = tile.outputs[outputIndex];

                    // Find the tile index of the 'to' connection
                    var to = 0;
                    for (to = 0; to < tiles.Length && tiles[to] != output.to.tile; to++);

                    var toTile = tiles[to];
                    
                    puzzle.wires[wireIndex++] = new SerializedWire {
                        from = tileIndex,
                        fromOrder = outputIndex,
                        fromOptions = output.from.options,
                        to = to,
                        toOrder = output.to.tile.inputs.FindIndex(w => w == output),
                        toOptions = output.to.options
                    };
                }

                puzzle.tiles = save;
            }

            return puzzle;
        }

        public void Load ()
        {
            GameManager.Instance.ClearTiles();

            if (tiles == null)
                return;

            var tilesObjects = new List<Tile>();
            foreach (var serializedTile in tiles)
            {
                //var tile =  GameManager.InstantiateTile(serializedTile.prefab, serializedTile.cell);
                //var editorInfo = tile.GetComponent<TileEditorInfo>();
                //editorInfo.guid = TileDatabaseTest.GetGuid(TileDatabase.instance.GetTile(serializedTile.prefab));
                Guid.TryParse(serializedTile.prefab, out var guid);
                var tile = GameManager.InstantiateTile(guid, serializedTile.cell);
                var editorInfo = tile.GetComponent<TileEditorInfo>();
                editorInfo.guid = guid;
                tilesObjects.Add(tile);
            }

            if (wires != null)
                foreach (var serializedWire in wires)
                {
                    var wire = GameManager.InstantiateWire(tilesObjects[serializedWire.from], tilesObjects[serializedWire.to]);
                    wire.from.tile.SetOutputIndex(wire, serializedWire.fromOrder);
                    wire.from.SetOptions(serializedWire.fromOptions);
                    wire.to.tile.SetInputIndex(wire, serializedWire.toOrder);
                    wire.to.SetOptions(serializedWire.toOptions);
                }

            for (int i = 0; i < tiles.Length; i++)
            {
                var serializedTile = tiles[i];
                if (serializedTile.properties == null)
                    continue;

                var tileEditorInfo = tilesObjects[i].GetComponent<TileEditorInfo>();                
                foreach (var serializedProperty in serializedTile.properties)
                    tileEditorInfo.SetEditableProperty(serializedProperty.name, serializedProperty.value);
            }
        }

        public static void Load(string filename) =>
            JsonUtility.FromJson<Puzzle>(File.ReadAllText(filename)).Load();
    }
}
