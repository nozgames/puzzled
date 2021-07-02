using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    public class UITilePaletteItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private Image _previewImage = null;
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
                        _nameText.text = _tile.gameObject.name;
                    _previewImage.sprite = DatabaseManager.GetPreview(_tile);
                    _previewImage.gameObject.SetActive(true);
                }
            }
        }
    }
}
