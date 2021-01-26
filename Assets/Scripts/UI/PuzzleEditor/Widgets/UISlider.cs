using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzled.Editor
{
    public class UISlider : UnityEngine.UI.Slider, IBeginDragHandler, IEndDragHandler
    {
        public event Action onBeginDrag;
        public event Action onEndDrag;

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
