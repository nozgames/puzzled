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
            _slider.value = float.TryParse(value, out var parsed) ? parsed : 0;
        }

        private void OnCommitValue()
        {
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, (int)_slider.value));
        }

        private void OnValueChanged (float value)
        {
            _input.SetTextWithoutNotify(((int)value).ToString());
            target.tile.SetPropertyValue(target.tileProperty.name, (int)_slider.value);
        }

        protected override void OnTargetChanged()
        {
            label = target.name;
            _slider.minValue = target.tileProperty.editable.range.x;
            _slider.maxValue = target.tileProperty.editable.range.y;
            _slider.SetValueWithoutNotify(target.GetValue<int>());
            _slider.onValueChanged.AddListener(OnValueChanged);

            _input.SetTextWithoutNotify(((int)_slider.value).ToString());
        }
    }
}
