using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIDecalItem : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private Image _previewImage = null;

        private Decal _decal;

        public Decal decal {
            get => _decal;
            set {
                _decal = value;
                if (_decal == null)
                    return;

                _nameText.text = _decal.sprite == null ? "None" : _decal.sprite.name;

                if (_decal.sprite != null)
                {
                    _previewImage.sprite = _decal.sprite;
                    _previewImage.enabled = true;
                } else
                    _previewImage.enabled = false;
            }
        }
    }
}
