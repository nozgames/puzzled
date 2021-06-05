using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIConfirmPopup : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _messageText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _titleText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _confirmText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _cancelText = null;

        [SerializeField] private Button _confirmButton = null;
        [SerializeField] private Button _cancelButton = null;

        private Action _onConfirm;
        private Action _onCancel;

        private void Awake()
        {
            _confirmButton.onClick.AddListener(() => {
                gameObject.SetActive(false);
                _onConfirm?.Invoke();
            });

            _cancelButton.onClick.AddListener(() => {
                gameObject.SetActive(false);
                _onCancel?.Invoke();
            });
        }


        public void Show(string message = null, string title = null, string confim = null, string cancel = null, Action onConfirm = null, Action onCancel = null)
        {
            _cancelButton.gameObject.SetActive(cancel != null);
            _confirmText.text = confim ?? "Yes";
            _cancelText.text = cancel ?? "No";
            _messageText.text = message;
            _titleText.text = title;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            gameObject.SetActive(true);
        }
    }
}
