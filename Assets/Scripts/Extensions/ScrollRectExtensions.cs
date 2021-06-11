using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public static class ScrollRectExtensions
    {
        // Shared array used to receive result of RectTransform.GetWorldCorners
        static Vector3[] corners = new Vector3[4];

        /// <summary>
        /// Transform the bounds of the current rect transform to the space of another transform.
        /// </summary>
        /// <param name="source">The rect to transform</param>
        /// <param name="target">The target space to transform to</param>
        /// <returns>The transformed bounds</returns>
        public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
        {
            // Based on code in ScrollRect's internal GetBounds and InternalGetBounds methods
            var bounds = new Bounds();
            if (source != null)
            {
                source.GetWorldCorners(corners);

                var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                var matrix = target.worldToLocalMatrix;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 v = matrix.MultiplyPoint3x4(corners[j]);
                    vMin = Vector3.Min(v, vMin);
                    vMax = Vector3.Max(v, vMax);
                }

                bounds = new Bounds(vMin, Vector3.zero);
                bounds.Encapsulate(vMax);
            }
            return bounds;
        }

        /// <summary>
        /// Normalize a distance to be used in verticalNormalizedPosition or horizontalNormalizedPosition.
        /// </summary>
        /// <param name="axis">Scroll axis, 0 = horizontal, 1 = vertical</param>
        /// <param name="distance">The distance in the scroll rect's view's coordiante space</param>
        /// <returns>The normalized scoll distance</returns>
        public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
        {
            // Based on code in ScrollRect's internal SetNormalizedPosition method
            var viewport = scrollRect.viewport;
            var viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
            var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

            var content = scrollRect.content;
            var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

            var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
            return distance / hiddenLength;
        }

        /// <summary>
        /// Scroll the target element to the vertical center of the scroll rect's viewport.
        /// Assumes the target element is part of the scroll rect's contents.
        /// </summary>
        /// <param name="scrollRect">Scroll rect to scroll</param>
        /// <param name="target">Element of the scroll rect's content to center vertically</param>
        public static void ScrollTo(this ScrollRect scrollRect, RectTransform target, int threshold=1)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(target);
            Canvas.ForceUpdateCanvases();

            // The scroll rect's view's space is used to calculate scroll position
            var view = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

            // Calcualte the scroll offset in the view's space
            var viewRect = view.rect;
            var elementBounds = target.TransformBoundsTo(view);
            var viewRectMin = viewRect.min.y + threshold * elementBounds.size.y;
            var viewRectMax = viewRect.max.y - threshold * elementBounds.size.y;

            var offset = 0.0f;
            if (elementBounds.min.y < viewRectMin)
                offset = viewRectMin - elementBounds.min.y;
            else if (elementBounds.max.y > viewRectMax)
                offset = viewRectMax - elementBounds.max.y;
            else
                return;
           
            // Normalize and apply the calculated offset
            var scrollPos = Mathf.Clamp(scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset), 0, 1);
            scrollRect.verticalNormalizedPosition = scrollPos;
        }

        public static void UpdateLayout(Transform xform)
        {
            Canvas.ForceUpdateCanvases();
            UpdateLayout_Internal(xform);
        }

        private static void UpdateLayout_Internal(Transform xform)
        {
            if (xform == null || xform.Equals(null))
            {
                return;
            }

            // Update children first
            for (int x = 0; x < xform.childCount; ++x)
            {
                UpdateLayout_Internal(xform.GetChild(x));
            }

            // Update any components that might resize UI elements
            foreach (var layout in xform.GetComponents<LayoutGroup>())
            {
                layout.CalculateLayoutInputVertical();
                layout.CalculateLayoutInputHorizontal();
            }
            foreach (var fitter in xform.GetComponents<ContentSizeFitter>())
            {
                fitter.SetLayoutVertical();
                fitter.SetLayoutHorizontal();
            }
        }
    }
}
