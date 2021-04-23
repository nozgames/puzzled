using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIColorEditor : UIPropertyEditor
    {
        [SerializeField] private Button _button = null;
        [SerializeField] private Image _image = null;
        [SerializeField] private RectTransform _alpha = null;

        private Color _color = Color.white;

        private void Awake()
        {
            _button.onClick.AddListener(() => {
                UIPuzzleEditor.instance.ChooseColor(_color, transform as RectTransform, (value, commit) => {
                    UpdateColor(value);
                    target.SetValue(value, commit);
                });
            });
        }

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();
            UpdateColor(target.GetValue<Color>());
        }

        private void UpdateColor(Color color)
        {
            _color = color;
            _image.color = new Color(_color.r, _color.g, _color.b);
            _alpha.anchorMax = new Vector2(_color.a, 1.0f);
        }
    }
}
