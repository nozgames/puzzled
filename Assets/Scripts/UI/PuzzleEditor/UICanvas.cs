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

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponentInParent<RectTransform>();
        }

        public void UnregisterAll ()
        {
            onLButtonClick = null;
            onLButtonDown = null;
            onLButtonUp = null;
            onLButtonDrag = null;
            onLButtonDragBegin = null;
            onLButtonDragEnd = null;
        }

        /// <summary>
        /// Convert a screen coordinate to a canvas coordinate
        /// </summary>
        public Vector2 ScreenToCanvas(Vector2 screen) =>
            _rect.InverseTransformPoint(screen);

        /// <summary>
        /// Convert a canvas coordinate to a screen coordinate
        /// </summary>
        public Vector2 CanvasToScreen(Vector2 canvas) =>
            _rect.TransformPoint(canvas);

        /// <summary>
        /// Convert a canvas position to a world ray
        /// </summary>
        public Ray CanvasToRay(Vector2 canvasPosition) =>
            CameraManager.camera.ScreenPointToRay(CanvasToScreen(canvasPosition));

        /// <summary>
        /// Convert a canvas position to a world position on the gound plane
        /// </summary>
        public Vector3 CanvasToWorld(Vector2 canvasPosition)
        {
            var ray = CanvasToRay(canvasPosition);
            if ((new Plane(Vector3.up, Vector3.zero)).Raycast(ray, out float enter))
                return ray.origin + ray.direction * enter;

            return Vector3.zero;
        }

        /// <summary>
        /// Convert a canvas position to a cell
        /// </summary>
        public Cell CanvasToCell(Vector2 canvasPosition) => CanvasToCell(canvasPosition, CellCoordinateSystem.Grid);

        /// <summary>
        /// Convert a canvas position to a cell in a specific coordinate system
        /// </summary>
        public Cell CanvasToCell(Vector2 canvasPosition, CellCoordinateSystem system) => 
            UIPuzzleEditor.instance.puzzle.grid.WorldToCell(CanvasToWorld(canvasPosition) + new Vector3(0.5f, 0, 0.5f), system);


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onLButtonClick?.Invoke(ScreenToCanvas(eventData.position));
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
                onLButtonDown?.Invoke(ScreenToCanvas(eventData.position));
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                onLButtonUp?.Invoke(ScreenToCanvas(eventData.position));
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            onScroll?.Invoke(ScreenToCanvas(eventData.position), eventData.scrollDelta);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onLButtonDrag?.Invoke(ScreenToCanvas(eventData.position), eventData.delta);
                    break;

                case PointerEventData.InputButton.Right:
                    onRButtonDrag?.Invoke(ScreenToCanvas(eventData.position), eventData.delta);
                    break;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onLButtonDragBegin?.Invoke(ScreenToCanvas(eventData.position));
                    break;
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onLButtonDragEnd?.Invoke(ScreenToCanvas(eventData.position));
                    break;
            }
        }
    }
}

