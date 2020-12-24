using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UITileItem : UIPaletteItem
    {
        [SerializeField] private RawImage preview = null;
        [SerializeField] private TMPro.TextMeshProUGUI nameText = null;

        private Tile _tile;

        public Tile tile {
            get => _tile;
            set {
                _tile = value;

                if(_tile != null)
                {
                    preview.texture = TileDatabase.GetPreview(tile);
                    nameText.text = tile.gameObject.name;
                }
            }
        }

        public override object value => _tile;
    }
}
