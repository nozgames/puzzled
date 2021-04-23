using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzled.Editor
{
    public class UISlider : UnityEngine.UI.Slider, IBeginDragHandler, IEndDragHandler
    {
        private TMPro.TMP_InputField _input = null;

        public event Action onBeginDrag;
        public event Action onEndDrag;

        public float inputMultiplier = 1.0f;

        protected override void Awake()
        {
            base.Awake();
            
            _input = transform.parent.GetComponentInChildren<TMPro.TMP_InputField>();
            if(_input != null)
            {
                onValueChanged.AddListener(OnValueChanged);
                _input.onSubmit.AddListener(OnInputChanged);
                _input.onEndEdit.AddListener(OnInputChanged);
            }
        }

        private void OnValueChanged(float value)
        {
            if (_input != null)
            {
                if(wholeNumbers)
                    _input.SetTextWithoutNotify(((int)(value * inputMultiplier)).ToString());
                else
                    _input.SetTextWithoutNotify((value * inputMultiplier).ToString());
            }
        }

        private void OnInputChanged(string value)
        {
            this.value = float.TryParse(value, out var parsed) ? parsed / inputMultiplier : 0;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke();
        }
    }
}
