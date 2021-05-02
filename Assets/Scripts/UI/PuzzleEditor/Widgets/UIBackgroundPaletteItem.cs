using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    public class UIBackgroundPaletteItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private Image _previewImage = null;

        private Background _background;

        public Background background {
            get => _background;
            set {
                _background = value;

                if (_background == null)
                {
                    _nameText.text = "None";
                    _previewImage.gameObject.SetActive(false);
                } else
                {
                    _nameText.text = _background.name;
                    _previewImage.color = _background.color;
                    _previewImage.gameObject.SetActive(true);
                }
            }
        }
    }
}
