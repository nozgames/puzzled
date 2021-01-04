using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzled
{
    class UISequenceStep : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMPro.TMP_InputField _input = null;
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;
        [SerializeField] private TMPro.TextMeshProUGUI _index = null;

        public event Action<UISequenceStep> onNameChanged;

        public string text {
            get => _text.text;
            set {
                _text.text = value;
            }
        }

        private void Awake()
        {
            _input.onEndEdit.AddListener(OnEndEdit);
            _input.onDeselect.AddListener(OnEndEdit);
        }

        private void OnEnable()
        {
            _input.gameObject.SetActive(false);
            _text.gameObject.SetActive(true);
            _index.text = (transform.GetSiblingIndex() + 1).ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2 && !_input.gameObject.activeSelf)
            {
                _input.text = _text.text;
                _input.gameObject.SetActive(true);
                _text.gameObject.SetActive(false);
                EventSystem.current.SetSelectedGameObject(_input.gameObject);
            }
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
