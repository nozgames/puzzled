using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIStringEditor : UIPropertyEditor
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

        protected override void OnTargetChanged()
        {
            label = target.name;
            input.SetTextWithoutNotify(target.GetValue<string>());
        }
    }
}
