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
            var option = ((TilePropertyEditorTarget)target);
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, newValue));
        }

        protected override void OnTargetChanged(object target)
        {
            var option = ((TilePropertyEditorTarget)target);
            toggle.SetIsOnWithoutNotify(option.GetValue<bool>());
            label = option.name;
        }
    }
}
