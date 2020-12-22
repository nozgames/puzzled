using UnityEngine;
using UnityEngine.UI;

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
            ((TilePropertyOption)target).SetValue(text);
        }

        protected override void OnTargetChanged(object target)
        {
            var option = ((TilePropertyOption)target);
            label = option.name;
            input.text = option.GetValue<string>();
        }
    }
}
