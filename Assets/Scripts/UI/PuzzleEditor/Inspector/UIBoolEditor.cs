using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIBoolEditor : UIPropertyEditor
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
            target.SetValue(newValue);
        }

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();
            toggle.SetIsOnWithoutNotify(target.GetValue<bool>());            
        }
    }
}
