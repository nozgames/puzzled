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

                _image.transform.localScale = new Vector3(_decal.isFlipped ? -1 : 1, 1.0f, 1.0f);
                _image.transform.localRotation = Quaternion.Euler(0, 0, _decal.rotation);
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
