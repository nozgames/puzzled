﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Puzzled.UI
{
    class UIWorldTransitionScreen : UIScreen, IPointerDownHandler
    {
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;
        [SerializeField] private RawImage _image = null;

        private World.Transition _transition;

        public System.Action callback { get; set; }

        public World.Transition transition {
            get => _transition;
            set {
                _transition = value;
                UpdateTransition();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            callback?.Invoke();
        }

        private void OnEnable()
        {
            UpdateTransition();
        }

        private void UpdateTransition()
        {
            if (_transition == null || !isActiveAndEnabled)
                return;

            _text.text = _transition.text;
            _text.gameObject.SetActive(!string.IsNullOrEmpty(_transition.text));

            _image.texture = _transition.texture;
            _image.gameObject.SetActive(_image.texture != null);
        }
    }
}
