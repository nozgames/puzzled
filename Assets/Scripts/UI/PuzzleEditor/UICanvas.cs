using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzled.Editor
{
    public class UICanvas : MonoBehaviour, 
        IPointerClickHandler, 
        IPointerEnterHandler, 
        IPointerExitHandler, 
        IPointerDownHandler, 
        IPointerUpHandler,
        IDragHandler,
        IBeginDragHandler,
        IEndDragHandler,
        IScrollHandler
    {
        public bool isMouseOver { get; private set; }

        public delegate void PositionDelegate (Vector2 position);
        public delegate void ScrollDelegate(Vector2 position, Vector2 delta);
        public delegate void DragDelegate(Vector2 position, Vector2 delta);

        public PositionDelegate onLButtonClick;
        public PositionDelegate onLButtonDown;
        public PositionDelegate onLButtonUp;
        public DragDelegate onLButtonDrag;
        public PositionDelegate onLButtonDragBegin;
        public PositionDelegate onLButtonDragEnd;
        public ScrollDelegate onScroll;
        public DragDelegate onRButtonDrag;
        public Action onEnter;
        public Action onExit;

        public void UnregisterAll ()
        {
            onLButtonClick = null;
            onLButtonDown = null;
            onLButtonUp = null;
            onLButtonDrag = null;
            onLButtonDragBegin = null;
            onLButtonDragEnd = null;
        }

        public Vector3 CanvasToWorld(Vector2 position)
        {
            var ray = CameraManager.camera.ScreenPointToRay(position);
            if ((new Plane(Vector3.up, Vector3.zero)).Raycast(ray, out float enter))
                return ray.origin + ray.direction * enter;

            return Vector3.zero;
        }

        public Cell CanvasToCell(Vector2 position) => CanvasToCell(position, CellCoordinateSystem.Grid);

        public Cell CanvasToCell(Vector2 position, CellCoordinateSystem system) => 
            UIPuzzleEditor.instance.puzzle.grid.WorldToCell(CanvasToWorld(position) + new Vector3(0.5f, 0, 0.5f), system);

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onLButtonClick?.Invoke(eventData.position);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            isMouseOver = true;
            onEnter?.Invoke();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;
            onExit?.Invoke();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onLButtonDown?.Invoke(eventData.position);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onLButtonUp?.Invoke(eventData.position);
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            onScroll?.Invoke(eventData.position, eventData.scrollDelta);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onLButtonDrag?.Invoke(eventData.position, eventData.delta);
                    break;

                case PointerEventData.InputButton.Right:
                    onRButtonDrag?.Invoke(eventData.position, eventData.delta);
                    break;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onLButtonDragBegin?.Invoke(eventData.position);
                    break;
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onLButtonDragEnd?.Invoke(eventData.position);
                    break;
            }
        }
    }
}

