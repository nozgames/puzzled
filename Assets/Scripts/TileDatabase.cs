using System.Linq;
using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Tile Databae", menuName = "Puzzled/Tile Database")]
    class TileDatabase : ScriptableObject 
    {
        [SerializeField] private Tile[] _prefabs = null;

        private void OnEnable()
        {
            tiles = _prefabs.Select(a => a.info)
                .Distinct()
                .OrderBy(ti => ti.name)
                .ToArray();
        }

        public Tile[] prefabs => _prefabs;

        public TileInfo[] tiles { get; private set; }

        public Tile GetTile (string guid) => prefabs.Where(p => p.guid == guid).FirstOrDefault();
    }
}
