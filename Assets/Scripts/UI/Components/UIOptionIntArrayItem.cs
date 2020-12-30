using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzled.Editor    
{
    class UIOptionIntArrayItem : UIListItem
    {
        [SerializeField] private TMPro.TMP_InputField _input = null;
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;

        public event Action<int> onValueChanged;

        public int value {
            get => int.Parse(_input.text);
            set {
                _input.SetTextWithoutNotify(value.ToString());
                _text.text = _input.text;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _input.onEndEdit.AddListener(OnEndEdit);
            _input.onDeselect.AddListener(OnEndEdit);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _input.gameObject.SetActive(false);
            _text.gameObject.SetActive(true);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2 && !_input.gameObject.activeSelf)
            {
                _input.text = _text.text;
                _input.gameObject.SetActive(true);
                _text.gameObject.SetActive(false);
                EventSystem.current.SetSelectedGameObject(_input.gameObject);
            } else
                base.OnPointerClick(eventData);
        }

        private void OnEndEdit(string value)
        {
            if (value != _text.text)
            {
                _text.text = value;
                onValueChanged?.Invoke(int.Parse(value));
            }

            _text.gameObject.SetActive(true);
            _input.gameObject.SetActive(false);            
        }
    }
}
