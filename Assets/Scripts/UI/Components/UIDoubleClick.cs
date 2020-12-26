using Puzzled.Editor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Puzzled
{
    public class UIDoubleClick : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        public UnityEvent onDoubleClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2 && eventData.button == PointerEventData.InputButton.Left)
                onDoubleClick?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var item = GetComponentInParent<UIListItem>();
            if (null != item)
                item.selected = true;
        }
    }
}
