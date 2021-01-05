using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    class UIKeypadPopupItem : MonoBehaviour
    {
        [SerializeField] private Image _image = null;
        [SerializeField] private Button _button = null;

        private Decal _decal;

        public Decal decal {
            get => _decal;
            set {
                _decal = value;

                if(_decal.sprite == null)
                {
                    _image.enabled = false;
                    return;
                }

                _image.sprite = _decal.sprite;

                _image.transform.localScale = new Vector3(_decal.flipHorizontal ? -1 : 1, _decal.flipVertical ? -1 : 1, 1);
                _image.transform.localRotation = Quaternion.Euler(0, 0, _decal.rotate ? -90 : 0);
                _image.enabled = true;
            }
        }

        public Button button => _button;

        private void OnEnable()
        {
            _image.enabled = false;
        }
    }
}
