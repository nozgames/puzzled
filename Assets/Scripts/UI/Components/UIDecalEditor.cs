using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Puzzled.Editor
{
    class UIDecalEditor : MonoBehaviour
    {        
        [SerializeField] private Image _preview = null;
        [SerializeField] private UIDoubleClick _button = null;

        private Decal _decal;

        public Action<Decal> onDecalChanged;

        public Decal decal {
            get => _decal;
            set {
                _decal = value;
                _preview.gameObject.SetActive(_decal != Decal.none);
                _preview.sprite = _decal.sprite;
            }
        }

        private void Awake()
        {
            _button.onDoubleClick.AddListener(() => {
                UIPuzzleEditor.instance.ChooseDecal((d) => {
                    decal = d;
                    onDecalChanged?.Invoke(decal);
                });
            });
        }
    }
}
