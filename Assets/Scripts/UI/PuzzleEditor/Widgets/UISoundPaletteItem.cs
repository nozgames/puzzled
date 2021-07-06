using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;
using UnityEngine.EventSystems;
using NoZ;

namespace Puzzled.Editor
{
    public class UISoundPaletteItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private Image _previewImage = null;
        [SerializeField] private Sprite _previewNone = null;
        [SerializeField] private Sprite _previewSound = null;

        private Sound _sound;

        public Sound sound {
            get => _sound;
            set {
                _sound = value;

                if (_sound.clip == null)
                {
                    _nameText.text = "None";
                    _previewImage.sprite = _previewNone;
                } else
                {
                    _nameText.text = _sound.clip.name;
                    _previewImage.sprite = _previewSound;
                }
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (sound.clip != null && eventData.clickCount == 1)
                AudioManager.Instance.Play(sound.clip);
        }
    }
}
