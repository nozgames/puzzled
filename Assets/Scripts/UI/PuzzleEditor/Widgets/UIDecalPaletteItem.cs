using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    public class UIDecalPaletteItem : UIListItem
    {
        [SerializeField] private RawImage _previewImage = null;
        [SerializeField] private RawImage _coloredPreviewImage = null;

        private Decal _decal;

        public Decal decal {
            get => _decal;
            set {
                _decal = value;

                if(_decal == null || _decal.texture == null)
                {
                    if(_decal.isImported)
                    {
                        _previewImage.gameObject.SetActive(false);
                        _coloredPreviewImage.gameObject.SetActive(true);
                        _coloredPreviewImage.texture = _decal.texture;
                    }
                    else
                    {
                        _coloredPreviewImage.gameObject.SetActive(false);
                        _previewImage.gameObject.SetActive(true);
                        _previewImage.texture = _decal.texture;
                    }

                }
                else
                {
                    _previewImage.texture = _decal.texture;
                    _previewImage.gameObject.SetActive(true);
                    _coloredPreviewImage.gameObject.SetActive(false);

                    if (!_decal.isAutoColor)
                        _previewImage.color = _decal.color;
                }
            }
        }
    }
}
