using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzled.Editor
{
    public enum TooltipDirection
    {
        Top,
        Bottom,
        Right,
        Left
    }

    public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TooltipDirection _direction = TooltipDirection.Bottom;
        [Multiline]
        [SerializeField] private string _text = "";
        [SerializeField] private float _delay = 0.0f;

        private Coroutine _delayCoroutine = null;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(_delay > 0.0f)
                _delayCoroutine = StartCoroutine(DoDelay());
            else
                UIPuzzleEditor.ShowTooltip(GetComponent<RectTransform>(), _text, _direction);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(null != _delayCoroutine)
                StopCoroutine(_delayCoroutine);
            _delayCoroutine = null;
            UIPuzzleEditor.HideTooltip();
        }

        private IEnumerator DoDelay ()
        {
            yield return new WaitForSeconds(_delay);
            UIPuzzleEditor.ShowTooltip(GetComponent<RectTransform>(), _text, _direction);
        }
    }
}
