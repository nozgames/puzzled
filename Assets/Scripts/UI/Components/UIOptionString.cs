using UnityEngine;

namespace Puzzled
{
    public class UIOptionString : UIOptionEditor
    {
        [SerializeField] private TMPro.TMP_InputField input = null;

        private void OnEnable()
        {
            input.onValueChanged.AddListener(OnInputValueChanged);
        }

        private void OnDisable()
        {
            input.onValueChanged.RemoveListener(OnInputValueChanged);
        }

        private void OnInputValueChanged(string text)
        {
            target.SetValue(text);
        }

        protected override void OnTargetChanged(TileEditorInfo.EditableProperty target)
        {
            input.text = target.GetValue();
        }
    }
}
