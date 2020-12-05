using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIOptionBool : UIOptionEditor
    {
        [SerializeField] private Toggle toggle = null;

        private void OnEnable()
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool newValue)
        {
            var editableProperty = ((TileEditorInfo.EditableProperty)target);
            editableProperty.SetValue(newValue.ToString());
        }

        protected override void OnTargetChanged(object target)
        {
            var editableProperty = ((TileEditorInfo.EditableProperty)target);
            toggle.isOn = bool.TryParse(editableProperty.GetValue(), out var value) ? value : false;
        }
    }
}
