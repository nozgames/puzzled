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
            target.SetValue(int.TryParse(text, out var parsed) ? parsed : 0);
        }

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();
            input.SetTextWithoutNotify(target.GetValue<int>().ToString());
        }
    }
}
