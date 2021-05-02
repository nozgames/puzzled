using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIStringEditor : UIPropertyEditor
    {
        [SerializeField] private TMPro.TMP_InputField input = null;
        [SerializeField] private TMPro.TextMeshProUGUI _placeholderText = null;

        private void OnEnable()
        {
            input.onEndEdit.AddListener(OnSubmitValue);
            input.onDeselect.AddListener(OnSubmitValue);
        }

        private void OnDisable()
        {
            input.onEndEdit.RemoveListener(OnSubmitValue);
            input.onDeselect.RemoveListener(OnSubmitValue);
        }

        private void OnSubmitValue(string text)
        {
            if (target.GetValue<string>() == text)
                return;

            target.SetValue(text);
        }

        protected override void OnTargetChanged()
        {
            input.SetTextWithoutNotify(target.GetValue<string>());

            if (_placeholderText != null && !string.IsNullOrEmpty(target.placeholder))
                _placeholderText.text = target.placeholder;
        }
    }
}
