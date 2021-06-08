using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    public class UIDecalPaletteItem : UIListItem
    {
        [SerializeField] private RawImage _previewImage = null;

        private Decal _decal;

        public Decal decal {
            get => _decal;
            set {
                _decal = value;

                if(_decal == null || _decal.texture == null)
                {
                    _previewImage.gameObject.SetActive(false);
                }
                else
                {
                    _previewImage.texture = _decal.texture;
                    _previewImage.gameObject.SetActive(true);

                    if (!_decal.isAutoColor)
                        _previewImage.color = _decal.color;
                }
            }
        }
    }
}
