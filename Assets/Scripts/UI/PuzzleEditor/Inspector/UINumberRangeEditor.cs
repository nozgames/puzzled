using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UINumberRangeEditor : UIPropertyEditor
    {
        [SerializeField] private UISlider _slider = null;
        [SerializeField] private TMPro.TMP_InputField _input = null;

        private void Awake()
        {            
            _slider.onEndDrag += OnCommitValue;
            _input.onSubmit.AddListener(OnInputChanged);
            _input.onEndEdit.AddListener(OnInputChanged);
        }

        private void OnInputChanged(string value)
        {
            float.TryParse(value, out var parsed);
            _slider.value = parsed;

            // Handle the case where value that was entered was out of range or formatted wrong
            if(_slider.value != parsed)
                _input.SetTextWithoutNotify(((int)_slider.value).ToString());
        }

        private void OnCommitValue()
        {
            target.SetValue((int)_slider.value);
        }

        private void OnValueChanged (float value)
        {
            _input.SetTextWithoutNotify(((int)value).ToString());
            target.SetValue((int)_slider.value, false);
        }

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            _slider.minValue = target.range.x;
            _slider.maxValue = target.range.y;
            _slider.SetValueWithoutNotify(target.GetValue<int>());
            _slider.onValueChanged.AddListener(OnValueChanged);

            _input.SetTextWithoutNotify(((int)_slider.value).ToString());
        }
    }
}
