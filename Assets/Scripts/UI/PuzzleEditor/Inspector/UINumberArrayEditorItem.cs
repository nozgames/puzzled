using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Puzzled.Editor    
{
    class UINumberArrayEditorItem : UIListItem
    {
        [SerializeField] private TMPro.TMP_InputField _input = null;
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;
        [SerializeField] private Button _deleteButton = null;
        [SerializeField] private TMPro.TextMeshProUGUI _index = null;

        public event Action<int> onValueChanged;
        public event Action<UINumberArrayEditorItem> onDeleted;

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

            _deleteButton.onClick.AddListener(() => { onDeleted?.Invoke(this);  });

            _input.onEndEdit.AddListener(OnEndEdit);
            _input.onDeselect.AddListener(OnEndEdit);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _input.gameObject.SetActive(false);
            _text.gameObject.SetActive(true);

            if (_index != null)
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
            } else
                base.OnPointerClick(eventData);
        }

        private void OnEndEdit(string value)
        {
            if (value != _text.text)
            {
                _text.text = value;
                onValueChanged?.Invoke(int.TryParse(value, out var parsed) ? parsed : 0);
            }

            _text.gameObject.SetActive(true);
            _input.gameObject.SetActive(false);            
        }
    }
}
