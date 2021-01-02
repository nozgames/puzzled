using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIOptionBool : UIPropertyEditor
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

        protected override void OnTargetChanged()
        {
            toggle.SetIsOnWithoutNotify(target.GetValue<bool>());
            label = target.name;
        }
    }
}
