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
        private class Tile
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

        [SerializeField] [HideInInspector] private Tile[] tiles;
        [SerializeField] [HideInInspector] private Connection[] connections;

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
            var actors = target.GetComponentsInChildren<PuzzledActor>();
            var save = new Tile[actors.Length];

            connections = new Connection[actors.Sum(a => a.connections.Length)];
            var connectionIndex = 0;
            for(int i=0; i<actors.Length; i++)
            {
                var actor = actors[i];
                save[i] = new Tile {
                    prefab = "",
                    cell = actor.Cell
                };

                foreach(var connectedActor in actor.connections)
                {
                    var to = 0;
                    for (to = 0; to < actors.Length && actors[to] != connectedActor; to++);
                    connections[connectionIndex++] = new Connection {
                        from = i,
                        to = i
                    };
                }
            }
        }
    }
}
