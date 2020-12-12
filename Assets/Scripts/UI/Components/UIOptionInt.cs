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
            ((TilePropertyOption)target).SetValue(text);
        }

        protected override void OnTargetChanged(object target)
        {
            var option = ((TilePropertyOption)target);
            label = option.name;
            input.text = option.GetValue();
        }
    }
}
