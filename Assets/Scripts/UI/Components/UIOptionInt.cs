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
            input.onValueChanged.AddListener(OnInputValueChanged);
            input.onValidateInput = OnValidateInput;
        }

        private void OnDisable()
        {
            input.onValueChanged.RemoveListener(OnInputValueChanged);
        }

        private char OnValidateInput(string text, int charIndex, char addedChar)
        {
            if (addedChar < '0' || addedChar > '9')
                return '\0';

            return addedChar;
        }

        private void OnInputValueChanged(string text)
        {
            var option = ((TilePropertyOption)target);
            var newValue = int.TryParse(text, out var parsed) ? parsed : 0;
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, newValue));
        }

        protected override void OnTargetChanged(object target)
        {
            var option = ((TilePropertyOption)target);
            label = option.name;
            input.SetTextWithoutNotify(option.GetValue<int>().ToString());
        }
    }
}
