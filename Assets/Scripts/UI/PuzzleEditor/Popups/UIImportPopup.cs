using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIImportPopup : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _title = null;
        [SerializeField] private UIListItem _itemPrefab = null;
        [SerializeField] private Button _okButton = null;
        [SerializeField] private Button _cancelButton = null;
        [SerializeField] private TMPro.TMP_InputField _path = null;

        private Action<string> _callback;

        private void Awake()
        {
            _okButton.onClick.AddListener(Done);
            _cancelButton.onClick.AddListener(() => _callback.Invoke(null));
            _path.onValueChanged.AddListener((value) => UpdateButtons());
        }

        public void Import (Action<string> callback)
        {
            _path.gameObject.SetActive(true);
            _callback = callback;
            _title.text = "Import";
            _path.text = "";
            UpdateButtons();
        }

        private void Done()
        {
            _callback.Invoke(_path.text);
        }

        private void UpdateButtons()
        {
            _okButton.interactable = !string.IsNullOrEmpty(_path.text) && File.Exists(_path.text);

            // TODO: copy the file to the world folder
            // TODO: Create a decal in the world decal list
            // TODO: generate a guid for the imported decal
        }
   }
}
