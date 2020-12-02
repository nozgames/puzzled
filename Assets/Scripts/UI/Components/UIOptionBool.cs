using System;
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
            target.SetValue(newValue.ToString());
        }

        protected override void OnTargetChanged(TileEditorInfo.EditableProperty target)
        {
            toggle.isOn = bool.TryParse(target.GetValue(), out var value) ? value : false;
        }
    }
}
