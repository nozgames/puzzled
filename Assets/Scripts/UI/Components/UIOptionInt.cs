using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIOptionInt : UIOptionEditor
    {
        [SerializeField] private TMPro.TMP_InputField input = null;

        private void OnEnable()
        {
            input.onEndEdit.AddListener(OnInputValueChanged);
        }

        private void OnDisable()
        {
            input.onEndEdit.RemoveListener(OnInputValueChanged);
        }

        private void OnInputValueChanged(string text)
        {
            var option = ((TilePropertyEditorTarget)target);
            var newValue = int.TryParse(text, out var parsed) ? parsed : 0;
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, newValue));
        }

        protected override void OnTargetChanged(object target)
        {
            var option = ((TilePropertyEditorTarget)target);
            label = option.name;
            input.SetTextWithoutNotify(option.GetValue<int>().ToString());
        }
    }
}
