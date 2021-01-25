using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Puzzled.Editor    
{
    class UIStringArrayEditorItem : UIListItem
    {
        [SerializeField] private TMPro.TMP_InputField _input = null;
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;
        [SerializeField] private TMPro.TextMeshProUGUI _index = null;
        [SerializeField] private Button _deleteButton = null;

        public event Action<string> onValueChanged;
        public event Action<UIStringArrayEditorItem> onDeleted;

        public string value {
            get => _input.text;
            set {
                _input.SetTextWithoutNotify(value);
                _text.text = _input.text;
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

            if(_index != null)
                _index.text = (transform.GetSiblingIndex() + 1).ToString();
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
                onValueChanged?.Invoke(value);
            }

            _text.gameObject.SetActive(true);
            _input.gameObject.SetActive(false);            
        }
    }
}
