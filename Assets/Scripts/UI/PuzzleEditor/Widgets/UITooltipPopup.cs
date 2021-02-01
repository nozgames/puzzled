using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UITooltipPopup : MonoBehaviour
    {
        [SerializeField] private RectTransform _arrow = null;
        [SerializeField] private float _arrowMargin = 20.0f;
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;        

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }


        public void Show (RectTransform sourceRect, string text, TooltipDirection direction)
        {
            var parentRect = transform.parent.GetComponent<RectTransform>().rect;

            gameObject.SetActive(true);
            _text.text = text;
            _rectTransform.anchorMin = _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = Vector2.zero;
            _arrow.anchoredPosition = Vector2.zero;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

            var source = sourceRect.TransformBoundsTo(transform.parent);
            var size = _rectTransform.rect.size;

            switch (direction)
            {
                case TooltipDirection.Right:
                    _rectTransform.anchoredPosition = new Vector2(source.max.x + _arrowMargin, source.center.y);
                    _rectTransform.pivot = _arrow.anchorMin = _arrow.anchorMax = new Vector2(0.0f, 0.5f);
                    break;

                case TooltipDirection.Left:
                    _rectTransform.anchoredPosition = new Vector2(source.min.x - _arrowMargin, source.center.y);
                    _rectTransform.pivot = _arrow.anchorMin = _arrow.anchorMax = new Vector2(1.0f, 0.5f);
                    break;

                case TooltipDirection.Top:
                    _rectTransform.anchoredPosition = new Vector2(source.center.x, source.max.y + _arrowMargin);
                    _rectTransform.pivot = _arrow.anchorMin = _arrow.anchorMax = new Vector2(0.5f, 0.0f);
                    break;

                case TooltipDirection.Bottom:
                    _rectTransform.anchoredPosition = new Vector2(source.center.x, source.min.y - _arrowMargin);
                    _rectTransform.pivot =  _arrow.anchorMin = _arrow.anchorMax = new Vector2(0.5f, 1.0f);
                    break;
            }

            // Correct positions if the tooltip does not fit on the screen
            switch (direction)
            {
                case TooltipDirection.Top:
                case TooltipDirection.Bottom:
                    if (_rectTransform.anchoredPosition.x + size.x * 0.5f > parentRect.max.x)
                    {
                        _arrow.anchorMax = _arrow.anchorMin = _rectTransform.pivot = new Vector2(1.0f, _arrow.anchorMin.y);
                        _arrow.anchoredPosition = new Vector2(-_arrowMargin, 0);
                        _rectTransform.anchoredPosition += new Vector2(_arrowMargin, 0);
                    } else if (_rectTransform.anchoredPosition.x - size.x * 0.5f < parentRect.min.x)
                    {
                        _arrow.anchorMax = _arrow.anchorMin = _rectTransform.pivot = new Vector2(0.0f, _arrow.anchorMin.y);
                        _arrow.anchoredPosition = new Vector2(_arrowMargin, 0);
                        _rectTransform.anchoredPosition -= new Vector2(_arrowMargin, 0);
                    }
                    break;

                case TooltipDirection.Right:
                case TooltipDirection.Left:
                    if (_rectTransform.anchoredPosition.y + size.y * 0.5f > parentRect.max.y)
                    {
                        _arrow.anchorMax = _arrow.anchorMin = _rectTransform.pivot = new Vector2(_arrow.anchorMin.x, 1.0f);
                        _arrow.anchoredPosition = new Vector2(0, -_arrowMargin);
                        _rectTransform.anchoredPosition += new Vector2(0, _arrowMargin);
                    } else if (_rectTransform.anchoredPosition.y - size.y * 0.5f < parentRect.min.y)
                    {
                        _arrow.anchorMax = _arrow.anchorMin = _rectTransform.pivot = new Vector2(_arrow.anchorMin.x, 0.0f);
                        _arrow.anchoredPosition = new Vector2(0, _arrowMargin);
                        _rectTransform.anchoredPosition -= new Vector2(0, _arrowMargin);
                    }
                    break;
            }

            _rectTransform.ForceUpdateRectTransforms();
        }
    }
}
