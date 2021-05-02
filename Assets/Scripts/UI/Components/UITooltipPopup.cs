using NoZ;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UITooltipPopup : MonoBehaviour
    {
        [SerializeField] private RectTransform _target = null;
        [SerializeField] private RectTransform _arrow = null;
        [SerializeField] private float _arrowMargin = 20.0f;
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;
        [SerializeField] private float _animationTime = 0.0f;

        private void OnDisable()
        {
            Tween.Stop(_target.gameObject);
        }

        public void Show(RectTransform sourceRect, string text, TooltipDirection direction)
        {
            var bounds = sourceRect.TransformBoundsTo(_target.parent);
            Show(new Rect(bounds.min, bounds.size), text, direction);
        }

        public void Show (Rect source, string text, TooltipDirection direction)
        {
            var parentRect = _target.parent.GetComponent<RectTransform>().rect;

            _target.gameObject.SetActive(true);
            _text.text = text;
            _target.anchorMin = _target.anchorMax = new Vector2(0.5f, 0.5f);
            _target.anchoredPosition = Vector2.zero;
            _arrow.anchoredPosition = Vector2.zero;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_target);

            var size = _target.rect.size;

            switch (direction)
            {
                case TooltipDirection.Right:
                    _target.anchoredPosition = new Vector2(source.max.x + _arrowMargin, source.center.y);
                    _target.pivot = _arrow.anchorMin = _arrow.anchorMax = new Vector2(0.0f, 0.5f);
                    break;

                case TooltipDirection.Left:
                    _target.anchoredPosition = new Vector2(source.min.x - _arrowMargin, source.center.y);
                    _target.pivot = _arrow.anchorMin = _arrow.anchorMax = new Vector2(1.0f, 0.5f);
                    break;

                case TooltipDirection.Top:
                    _target.anchoredPosition = new Vector2(source.center.x, source.max.y + _arrowMargin);
                    _target.pivot = _arrow.anchorMin = _arrow.anchorMax = new Vector2(0.5f, 0.0f);
                    break;

                case TooltipDirection.Bottom:
                    _target.anchoredPosition = new Vector2(source.center.x, source.min.y - _arrowMargin);
                    _target.pivot =  _arrow.anchorMin = _arrow.anchorMax = new Vector2(0.5f, 1.0f);
                    break;
            }

            // Correct positions if the tooltip does not fit on the screen
            switch (direction)
            {
                case TooltipDirection.Top:
                case TooltipDirection.Bottom:
                    if (_target.anchoredPosition.x + size.x * 0.5f > parentRect.max.x)
                    {
                        _arrow.anchorMax = _arrow.anchorMin = _target.pivot = new Vector2(1.0f, _arrow.anchorMin.y);
                        _arrow.anchoredPosition = new Vector2(-_arrowMargin, 0);
                        _target.anchoredPosition += new Vector2(_arrowMargin, 0);
                    } else if (_target.anchoredPosition.x - size.x * 0.5f < parentRect.min.x)
                    {
                        _arrow.anchorMax = _arrow.anchorMin = _target.pivot = new Vector2(0.0f, _arrow.anchorMin.y);
                        _arrow.anchoredPosition = new Vector2(_arrowMargin, 0);
                        _target.anchoredPosition -= new Vector2(_arrowMargin, 0);
                    }
                    break;

                case TooltipDirection.Right:
                case TooltipDirection.Left:
                    if (_target.anchoredPosition.y + size.y * 0.5f > parentRect.max.y)
                    {
                        _arrow.anchorMax = _arrow.anchorMin = _target.pivot = new Vector2(_arrow.anchorMin.x, 1.0f);
                        _arrow.anchoredPosition = new Vector2(0, -_arrowMargin);
                        _target.anchoredPosition += new Vector2(0, _arrowMargin);
                    } else if (_target.anchoredPosition.y - size.y * 0.5f < parentRect.min.y)
                    {
                        _arrow.anchorMax = _arrow.anchorMin = _target.pivot = new Vector2(_arrow.anchorMin.x, 0.0f);
                        _arrow.anchoredPosition = new Vector2(0, _arrowMargin);
                        _target.anchoredPosition -= new Vector2(0, _arrowMargin);
                    }
                    break;
            }

            _target.ForceUpdateRectTransforms();

            if (_animationTime > 0.0f)
                Tween.Scale(0, 1).EaseOutElastic(1,2).Key("Animate").Duration(_animationTime).Start(_target.gameObject);
        }

        public void Hide()
        {
            _target.gameObject.SetActive(false);
        }
    }
}
