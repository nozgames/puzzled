using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UISoundPaletteItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private Image _previewImage = null;

        private Sound _sound;

        public Sound sound {
            get => _sound;
            set {
                _sound = value;

                if (_sound.clip == null)
                {
                    _nameText.text = "None";
                    _previewImage.gameObject.SetActive(false);
                } else
                {
                    _nameText.text = _sound.clip.name;
                    //_previewImage.sprite = _sound.sprite;
                    //_previewImage.gameObject.SetActive(true);
                }
            }
        }
    }
}
