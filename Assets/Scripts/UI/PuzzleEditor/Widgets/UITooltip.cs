using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzled.UI
{
    public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TooltipDirection _direction = TooltipDirection.Bottom;
        [Multiline]
        [SerializeField] private string _text = "";
        [SerializeField] private float _delay = 0.0f;

        private Coroutine _delayCoroutine = null;
        private UITooltipPopup _popup = null;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_delay > 0.0f)
                _delayCoroutine = StartCoroutine(DoDelay());
            else
                ShowPopup();                
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(null != _delayCoroutine)
                StopCoroutine(_delayCoroutine);

            _delayCoroutine = null;

            if(null != _popup)
                _popup.Hide();
        }

        private IEnumerator DoDelay ()
        {
            yield return new WaitForSeconds(_delay);
            ShowPopup();
        }

        private void ShowPopup()
        {
            if (_popup == null)
            {
                _popup = GetComponentInParent<UITooltipPopup>();
                if (null == _popup)
                    return;
            }
                
            _popup.Show(GetComponent<RectTransform>(), _text, _direction);
        }
    }
}
