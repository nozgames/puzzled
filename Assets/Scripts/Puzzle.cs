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
            public Cell cell;
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

        public static void Save (Tile[] tiles, string filename)
        {
            File.WriteAllText(filename, JsonUtility.ToJson(Save(tiles)));
        }

        public static Puzzle Save (Tile[] tiles)
        {
            var puzzle = new Puzzle();
            var save = new SerializedTile[tiles.Length];

            puzzle.wires = new SerializedWire[tiles.Sum(t => t.outputCount)];
            var wireIndex = 0;
            for(int tileIndex=0; tileIndex<tiles.Length; tileIndex++)
            {
                var tile = tiles[tileIndex];
                save[tileIndex] = new SerializedTile {
                    prefab = tile.guid.ToString(),
                    cell = tile.cell,
                    properties = tile.properties?
                        .Select(p => new SerializedProperty { name = p.property.Name, value = p.GetValue(tile) })
                        .Where(p => !string.IsNullOrEmpty(p.value))
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

        public void Load()
        {
            GameManager.UnloadPuzzle();

            if (tiles == null)
                return;

            var tilesObjects = new List<Tile>();
            foreach (var serializedTile in tiles)
            {
                Guid.TryParse(serializedTile.prefab, out var guid);
                tilesObjects.Add(GameManager.InstantiateTile(guid, serializedTile.cell));
            }

            if (wires != null)
            {
                // Create the wires
                var wireObjects = new List<Wire>();
                foreach (var serializedWire in wires)
                {
                    var wire = GameManager.InstantiateWire(tilesObjects[serializedWire.from], tilesObjects[serializedWire.to]);
                    wireObjects.Add(wire);
                    if (null == wire)
                        continue;

                    wire.from.SetOptions(serializedWire.fromOptions);
                    wire.to.SetOptions(serializedWire.toOptions);
                }

                // Sort the wires
                for (int wireIndex = 0; wireIndex < wireObjects.Count; wireIndex++)
                {
                    var wire = wireObjects[wireIndex];
                    if (wire == null)
                        continue;

                    wire.from.tile.SetOutputIndex(wire, wires[wireIndex].fromOrder, true);
                    wire.to.tile.SetInputIndex(wire, wires[wireIndex].toOrder, true);
                }
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                var serializedTile = tiles[i];
                if (serializedTile.properties == null)
                    continue;

                if (tilesObjects[i] == null)
                    continue;

                foreach (var serializedProperty in serializedTile.properties)
                    tilesObjects[i].SetProperty(serializedProperty.name, serializedProperty.value);
            }

            // Send a start event to all tiles
            var start = new StartEvent();
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tilesObjects[i] == null)
                    continue;

                tilesObjects[i].Send(start);
            }

            Debug.Log($"Puzzled Loaded: [{tiles.Length} tiles, {wires.Length} wires]");
        }

        public static void Load(string filename) =>
            JsonUtility.FromJson<Puzzle>(File.ReadAllText(filename)).Load();
    }
}
