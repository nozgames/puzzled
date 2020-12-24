using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UITilePaletteItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private RawImage _previewImage = null;

        private Tile _tile;

        public Tile tile {
            get => _tile;
            set {
                _tile = value;

                if (_tile == null)
                {
                    _nameText.text = "None";
                    _previewImage.gameObject.SetActive(false);
                } else
                {
                    _nameText.text = _tile.gameObject.name;
                    _previewImage.texture = TileDatabase.GetPreview(_tile);
                    _previewImage.gameObject.SetActive(true);
                }
            }
        }
    }
}
