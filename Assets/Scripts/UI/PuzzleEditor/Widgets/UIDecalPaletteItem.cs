using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIDecalPaletteItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private RawImage _previewImage = null;

        private Decal _decal;

        public Decal decal {
            get => _decal;
            set {
                _decal = value;

                if(_decal == null || _decal.texture == null)
                {
                    _nameText.text = "None";
                    _previewImage.gameObject.SetActive(false);
                }
                else
                {
                    _nameText.text = _decal.name;                    
                    _previewImage.texture = _decal.texture;
                    _previewImage.gameObject.SetActive(true);

                    if (!_decal.isAutoColor)
                        _previewImage.color = _decal.color;
                }
            }
        }
    }
}
