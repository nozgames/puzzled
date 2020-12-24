using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIDecalPaletteItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private Image _previewImage = null;

        private Decal _decal;

        public Decal decal {
            get => _decal;
            set {
                _decal = value;

                if(_decal == null || _decal.sprite == null)
                {
                    _nameText.text = "None";
                    _previewImage.gameObject.SetActive(false);
                }
                else
                {
                    _nameText.text = _decal.sprite.name;
                    _previewImage.sprite = _decal.sprite;
                    _previewImage.gameObject.SetActive(true);
                }
            }
        }
    }
}
