using System;
using UnityEngine;

namespace Puzzled.Editor
{
    public class UITileSelector : MonoBehaviour
    {
        [SerializeField] private UITileSelectorTile[] _tiles = null;

        private Action<Tile> _callback;

        private void Awake()
        {
            foreach (var tile in _tiles)
                tile.button.onClick.AddListener(() => {
                    OnClickTile(tile);
                });
        }

        private void OnClickTile(UITileSelectorTile tile)
        {
            _callback?.Invoke(tile.tile);
        }

        public void Open(Tile[] tilesTo, Action<Tile> callback)
        {
            _callback = callback;

            foreach (var tile in _tiles)
                tile.gameObject.SetActive(false);

            for (int i = 0; i < tilesTo.Length; i++)
            {
                _tiles[i].tile = tilesTo[i];
                _tiles[i].gameObject.SetActive(true);
            }
        }
    }
}
