using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    class UIDecalPreview : MonoBehaviour
    {        
        [SerializeField] private RawImage _preview = null;
        [SerializeField] private Image _none = null;

        private Color _autoColor;
        private Decal _decal;

        public Decal decal {
            get => _decal;
            set {
                _decal = value;

                if (isActiveAndEnabled)
                    UpdatePreview();
            }
        }

        private void Awake()
        {
            _autoColor = _preview.color;
        }

        private void OnEnable()
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            _none.gameObject.SetActive(_decal == Decal.none);
            _preview.gameObject.SetActive(_decal != Decal.none);
            _preview.texture = _decal.texture;
            _preview.color = _decal.isAutoColor ? _autoColor : _decal.color;
            _preview.transform.localRotation = Quaternion.Euler(0, 0, _decal.rotation);
            _preview.transform.localScale = new Vector3(_decal.isFlipped ? -1 : 1, 1, 1) * _decal.scale;
        }
    }
}
