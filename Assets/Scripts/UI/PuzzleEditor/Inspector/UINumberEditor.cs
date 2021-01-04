using UnityEngine;

namespace Puzzled
{
    public class UINumberEditor : UIPropertyEditor
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
            var newValue = int.TryParse(text, out var parsed) ? parsed : 0;
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, newValue));
        }

        protected override void OnTargetChanged()
        {
            label = target.name;
            input.SetTextWithoutNotify(target.GetValue<int>().ToString());
        }
    }
}
