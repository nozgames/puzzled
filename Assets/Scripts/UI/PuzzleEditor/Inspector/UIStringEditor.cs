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
            input.onDeselect.AddListener(OnSubmitValue);
        }

        private void OnDisable()
        {
            input.onEndEdit.RemoveListener(OnSubmitValue);
            input.onDeselect.RemoveListener(OnSubmitValue);
        }

        private void OnSubmitValue(string text)
        {
            if (target.GetValue<string>() == text)
                return;

            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, text));
        }

        protected override void OnTargetChanged()
        {
            label = target.name;
            input.SetTextWithoutNotify(target.GetValue<string>());
        }
    }
}
