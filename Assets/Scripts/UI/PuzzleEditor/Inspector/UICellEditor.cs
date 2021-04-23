using UnityEngine;

namespace Puzzled.Editor
{
    class UICellEditor : UIPropertyEditor
    {
        [SerializeField] private TMPro.TMP_InputField _inputX = null;
        [SerializeField] private TMPro.TMP_InputField _inputY = null;

        private void Awake()
        {
            _inputX.onEndEdit.AddListener(OnInputValueChanged);
            _inputY.onEndEdit.AddListener(OnInputValueChanged);
        }

        private void OnInputValueChanged(string text)
        {
            target.SetValue(new Cell(
                        int.TryParse(_inputX.text, out var parsedX) ? parsedX : 0,
                        int.TryParse(_inputY.text, out var parsedY) ? parsedY : 0
            ));
        }

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            var cell = target.GetValue<Cell>();
            _inputX.SetTextWithoutNotify(cell.x.ToString());
            _inputY.SetTextWithoutNotify(cell.y.ToString());
        }
    }
}
