using System;
using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    class UIDecalEditor : MonoBehaviour
    {        
        [SerializeField] private Image _preview = null;
        [SerializeField] private UIDoubleClick _button = null;
        [SerializeField] private UIRadio _toggleFlipX = null;
        [SerializeField] private UIRadio _toggleFlipY = null;
        [SerializeField] private UIRadio _toggleRotate = null;
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;

        private Decal _decal;

        public Action<Decal> onDecalChanged;

        public bool interactable { get; set; } = true;

        public Decal decal {
            get => _decal;
            set {
                _decal = value;
                _preview.gameObject.SetActive(_decal != Decal.none);
                _preview.sprite = _decal.sprite;
                _nameText.text = _decal.sprite == null ? "None" : _decal.sprite.name;

                _toggleFlipX.SetIsOnWithoutNotify((_decal.flags & DecalFlags.FlipHorizontal) == DecalFlags.FlipHorizontal);
                _toggleFlipY.SetIsOnWithoutNotify((_decal.flags & DecalFlags.FlipVertical) == DecalFlags.FlipVertical);
                _toggleRotate.SetIsOnWithoutNotify((_decal.flags & DecalFlags.Rotate) == DecalFlags.Rotate);
                _toggleFlipX.gameObject.SetActive(interactable && _decal != Decal.none);
                _toggleFlipY.gameObject.SetActive(interactable && _decal != Decal.none);
                _toggleRotate.gameObject.SetActive(interactable && _decal != Decal.none);

                _preview.transform.localScale = new Vector3(_toggleFlipX.isOn ? -1 : 1, _toggleFlipY.isOn?-1:1, 1);
                _preview.transform.localRotation = Quaternion.Euler(0, 0, _toggleRotate.isOn ? -90 : 0);
            }
        }

        private void Awake()
        {
            _toggleFlipX.onValueChanged.AddListener((v) => {
                if (v)
                    _decal.flags |= DecalFlags.FlipHorizontal;
                else
                    _decal.flags &= ~DecalFlags.FlipHorizontal;

                onDecalChanged?.Invoke(_decal);
            });

            _toggleFlipY.onValueChanged.AddListener((v) => {
                if (v)
                    _decal.flags |= DecalFlags.FlipVertical;
                else
                    _decal.flags &= ~DecalFlags.FlipVertical;

                onDecalChanged?.Invoke(_decal);
            });

            _toggleRotate.onValueChanged.AddListener((v) => {
                if (v)
                    _decal.flags |= DecalFlags.Rotate;
                else
                    _decal.flags &= ~DecalFlags.Rotate;

                onDecalChanged?.Invoke(_decal);
            });

            _button.onDoubleClick.AddListener(() => {
                if (!interactable)
                    return;

                UIPuzzleEditor.instance.ChooseDecal(decal, (d) => {
                    decal = d;
                    onDecalChanged?.Invoke(decal);
                });
            });
        }
    }
}
