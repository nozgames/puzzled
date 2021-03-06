﻿using System;
using Puzzled.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    class UISequenceStep : UIListItem, IPointerClickHandler
    {
        [SerializeField] private TMPro.TMP_InputField _input = null;
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;
        [SerializeField] private TMPro.TextMeshProUGUI _index = null;
        [SerializeField] private Button _deleteButton = null;

        public event Action<UISequenceStep> onNameChanged;
        public event Action<UISequenceStep> onDeleted;

        public string text {
            get => _text.text;
            set {
                _text.text = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _input.onEndEdit.AddListener(OnEndEdit);
            _input.onDeselect.AddListener(OnEndEdit);

            _deleteButton.onClick.AddListener(() => {
                onDeleted?.Invoke(this);
            });
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _input.gameObject.SetActive(false);
            _text.gameObject.SetActive(true);
            _index.text = transform.GetSiblingIndex().ToString();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {            
            if (eventData.clickCount == 2 && !_input.gameObject.activeSelf)
            {
                _input.text = _text.text;
                _input.gameObject.SetActive(true);
                _text.gameObject.SetActive(false);
                EventSystem.current.SetSelectedGameObject(_input.gameObject);
            }
            else
                base.OnPointerClick(eventData);
        }

        private void OnEndEdit(string value)
        {
            if (value != _text.text)
            {
                _text.text = value;
                onNameChanged?.Invoke(this);
            }

            _text.gameObject.SetActive(true);
            _input.gameObject.SetActive(false);
        }
    }
}
