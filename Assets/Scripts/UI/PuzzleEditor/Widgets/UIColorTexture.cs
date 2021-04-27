using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Puzzled.Editor
{
    public class UIColorTexture : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private RectTransform rectTransform;

        public UnityEvent<float,float> onValueChanged = new UnityEvent<float,float>();

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateValues(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateValues(eventData);
        }

        private void UpdateValues(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var point);
            var ratio = (point - rectTransform.rect.min) / rectTransform.rect.size;
            onValueChanged.Invoke(ratio.x, ratio.y);
        }
    }
}
