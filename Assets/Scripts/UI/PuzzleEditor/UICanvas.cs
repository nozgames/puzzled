using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Puzzled.PuzzleEditor
{
    public class UICanvas : MonoBehaviour, IPointerDownHandler
    {
        public UnityEvent<Vector2Int> pointerDown; 

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDown.Invoke(
                GameManager.WorldToCell(Camera.main.ScreenToWorldPoint(eventData.position) + new Vector3(0.5f, 0.5f, 0)));
        }
    }
}

