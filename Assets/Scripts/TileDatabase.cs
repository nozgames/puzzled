using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Tile Databae", menuName = "Puzzled/Tile Database")]
    class TileDatabase : ScriptableObject 
    {
        [SerializeField] private TileInfo[] _tiles = null;

        public TileInfo[] tiles => _tiles;
    }
}
