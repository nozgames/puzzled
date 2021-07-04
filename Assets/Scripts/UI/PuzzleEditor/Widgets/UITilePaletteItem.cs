using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    public class UITilePaletteItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private Image _previewImage = null;
        [SerializeField] private Image _coloredPreviewImage = null;
        [SerializeField] private UITooltip _tooltip = null;

        private Tile _tile;

        public Tile tile {
            get => _tile;
            set {
                _tile = value;

                if (_tile == null)
                {
                    if (_nameText != null)
                        _nameText.text = "None";
                    _previewImage.gameObject.SetActive(false);
                } 
                else
                {
                    if (_tooltip != null)
                        _tooltip.text = _tile.name;

                    if (_nameText != null)
                        _nameText.text = _tile.displayName;

                    var sprite = DatabaseManager.GetPreview(_tile);
                    if (sprite.name == "@")
                    {
                        _coloredPreviewImage.sprite = sprite;
                        _coloredPreviewImage.gameObject.SetActive(true);
                        _previewImage.gameObject.SetActive(false);
                    }
                    else
                    {
                        _previewImage.sprite = sprite;
                        _previewImage.gameObject.SetActive(true);
                        _coloredPreviewImage.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
