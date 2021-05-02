using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UINamePopup : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _titleText = null;
        [SerializeField] private TMPro.TMP_InputField _nameField = null;
        [SerializeField] private TMPro.TextMeshProUGUI _commitText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _placeholderText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _errorText = null;

        [SerializeField] private Button _commitButton = null;
        [SerializeField] private Button _closeButton = null;

        private Func<string, string> _onCommit;
        private Action _onCancel;

        public string title {
            get => _titleText.text;
            set => _titleText.text = value;
        }

        public string value {
            get => _nameField.text;
            set => _nameField.text = value;
        }

        public string commit {
            get => _commitText.text;
            set => _commitText.text = value;
        }

        public string placeholder {
            get => _placeholderText.text;
            set => _placeholderText.text = value;
        }

        public string error {
            get => _errorText.text;
            set {
                _errorText.text = value;
                _errorText.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }

        private void Awake()
        {
            _nameField.onValueChanged.AddListener((value) => {
                error = "";
                UpdateCommitButton();
            });

            _nameField.onSubmit.AddListener((value) => {
                Commit(value);
            });

            _commitButton.onClick.AddListener(() => {
                Commit(value);
            });

            _closeButton.onClick.AddListener(() => {
                gameObject.SetActive(false);
                _onCancel?.Invoke();
            });
        }

        private void Commit(string value)
        {
            var error = _onCommit?.Invoke(value);
            if (error != null)
            {
                this.error = error;
                _commitButton.Select();
                _nameField.Select();
                return;
            }

            gameObject.SetActive(false);
        }

        private void UpdateCommitButton()
        {
            _commitButton.interactable = value.Length > 0;
        }

        public void Show (string value = null, string title = null, string commit = null, string placeholder = null, Func<string, string> onCommit = null, Action onCancel = null)
        {
            this.value = value;
            this.commit = commit;
            this.title = title;
            this.placeholder = placeholder;
            this.error = "";
            _onCommit = onCommit;
            _onCancel = onCancel;
            UpdateCommitButton();
            gameObject.SetActive(true);
            _nameField.Select();
        }
    }
}
