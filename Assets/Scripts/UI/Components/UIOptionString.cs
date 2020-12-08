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
            var editableProperty = ((TileEditorInfo.EditableProperty)target);
            editableProperty.SetValue(text);
        }

        protected override void OnTargetChanged(object target)
        {
            var editableProperty = ((TileEditorInfo.EditableProperty)target);
            label = NicifyName(editableProperty.property.Name);
            input.text = editableProperty.GetValue();
        }
    }
}
