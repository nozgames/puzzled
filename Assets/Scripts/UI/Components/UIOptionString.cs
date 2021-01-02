using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIOptionString : UIOptionEditor
    {
        [SerializeField] private TMPro.TMP_InputField input = null;

        private void OnEnable()
        {
            input.onEndEdit.AddListener(OnSubmitValue);
        }

        private void OnDisable()
        {
            input.onEndEdit.RemoveListener(OnSubmitValue);
        }

        private void OnSubmitValue(string text)
        {
            var option = ((TilePropertyEditorTarget)target);
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, text));
        }

        protected override void OnTargetChanged(object target)
        {
            var option = ((TilePropertyEditorTarget)target);
            label = option.name;
            input.SetTextWithoutNotify(option.GetValue<string>());
        }
    }
}
