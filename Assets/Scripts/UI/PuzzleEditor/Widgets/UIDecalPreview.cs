using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    class UIDecalPreview : MonoBehaviour
    {        
        [SerializeField] private Image _preview = null;

        private Color _autoColor;
        private Decal _decal;

        private void Awake()
        {
            _autoColor = _preview.color;
        }

        public Decal decal {
            get => _decal;
            set {
                _decal = value;
                _preview.gameObject.SetActive(_decal != Decal.none);
                _preview.sprite = _decal.sprite;
                _preview.color = value.isAutoColor ? _autoColor : value.color;
                _preview.transform.localRotation = Quaternion.Euler(0, 0, _decal.rotation);
                _preview.transform.localScale = new Vector3(_decal.isFlipped ? -1 : 1, 1, 1) * _decal.scale;
            }
        }
    }
}
