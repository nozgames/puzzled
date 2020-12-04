using System;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class TileDatabase : AddressableDatabase<GameObject>
    {
        private static TileDatabase instance = null;

        private void Awake()
        {
            instance = this;
        }

        public static Tile GetTile(Guid guid) => instance.GetAsset(guid).GetComponent<Tile>();

        public static Guid GetGuid(Tile tile) => instance.GetGuid(tile.gameObject);

        public static Tile[] GetTiles() => instance._cache.Values.Select(t => t.GetComponent<Tile>()).ToArray();

        protected override string label => "tile";
    }
}
