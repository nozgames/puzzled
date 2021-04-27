using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIDecalEditor : UIPropertyEditor
    {
        [SerializeField] private UIDecalPreview _preview = null;
        [SerializeField] private UIDecalPreview _decalButtonPreview = null;
        [SerializeField] private Button _decalButton = null;
        [SerializeField] private Button _rotateButton = null;
        [SerializeField] private UINumberRangeEditor _scale = null;
        [SerializeField] private UINumberRangeEditor _rotation = null;
        [SerializeField] private UINumberRangeEditor _smoothness = null;
        [SerializeField] private UIBoolEditor _flipped = null;
        [SerializeField] private UIBoolEditor _autoColor = null;
        [SerializeField] private UIColorEditor _color = null;

        private object _boxedDecal;

        private class DecalPropertyTarget : IPropertyEditorTarget
        {
            private PropertyInfo _property = null;
            private UIDecalEditor _editor = null;

            private object _decal => _editor._boxedDecal;

            public System.Action<bool> onValueChanged { get; private set; }

            public string id => _property.Name;

            public string name => _property.Name.NicifyName();

            public string placeholder { get; set; }

            public Vector2Int range { get; set; } = Vector2Int.zero;

            public object GetValue() => _property.GetValue(_decal);

            public float floatScale { get; set; } = 100.0f;

            public T GetValue<T>()
            {
                if(typeof(T) == typeof(int) && _property.PropertyType == typeof(float))
                    return (T)(object)(int)((float)_property.GetValue(_decal) * floatScale);

                return (T)_property.GetValue(_decal);
            }

            public void SetValue(object value, bool commit = true)
            {
                if(value.GetType() == typeof(int) && _property.PropertyType == typeof(float))
                    _property.SetValue(_decal, (float)((int)value / floatScale));
                else
                    _property.SetValue(_decal, value);

                onValueChanged?.Invoke(commit);
            }

            public DecalPropertyTarget (UIDecalEditor editor, PropertyInfo property, System.Action<bool> onValueChanged)
            {
                _editor = editor;
                _property = property;
                this.onValueChanged = onValueChanged;
            }
        }

        private void Awake()
        {
            _decalButton.onClick.AddListener(() => {
                UIPuzzleEditor.instance.ChooseDecal((Decal)_boxedDecal, (decal) => {
                    var newDecal = (Decal)_boxedDecal;
                    newDecal.SetTexture(decal);
                    _boxedDecal = newDecal;
                    OnBoxedValueChanged(true);
                });
            });

            _rotateButton.onClick.AddListener(() => {
                var decal = (Decal)_boxedDecal;
                decal.rotation = (((int)(decal.rotation / 45.0f) + 1) % 8) * 45.0f;
                _boxedDecal = decal;
                OnBoxedValueChanged(true);                
            });            
        }

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            _boxedDecal = target.GetValue();
            _preview.decal = (Decal)_boxedDecal;
            _decalButtonPreview.decal = new Decal(_preview.decal.guid, _preview.decal.texture);

            _scale.target = new DecalPropertyTarget(this, _boxedDecal.GetType().GetProperty("scale"), OnBoxedValueChanged) { range = new Vector2Int(1, 100) };
            _rotation.target = new DecalPropertyTarget(this, _boxedDecal.GetType().GetProperty("rotation"), OnBoxedValueChanged) { range = new Vector2Int(0, 360), floatScale = 1.0f };
            _smoothness.target = new DecalPropertyTarget(this, _boxedDecal.GetType().GetProperty("smoothness"), OnBoxedValueChanged) { range = new Vector2Int(0, 100) };
            _flipped.target = new DecalPropertyTarget(this, _boxedDecal.GetType().GetProperty("isFlipped"), OnBoxedValueChanged);
            _autoColor.target = new DecalPropertyTarget(this, _boxedDecal.GetType().GetProperty("isAutoColor"), OnBoxedValueChanged);
            _color.target = new DecalPropertyTarget(this, _boxedDecal.GetType().GetProperty("color"), OnBoxedValueChanged);

            UpdateControls();
        }

        private void OnBoxedValueChanged(bool commit)
        {
            _preview.decal = (Decal)_boxedDecal;
            _decalButtonPreview.decal = new Decal(_preview.decal.guid, _preview.decal.texture);
            target.SetValue(_boxedDecal, commit);
            UpdateControls();
        }

        private void UpdateControls()
        {
            var hasDecal = _preview.decal != Decal.none;
            _scale.gameObject.SetActive(hasDecal);
            _rotation.gameObject.SetActive(hasDecal);
            _flipped.gameObject.SetActive(hasDecal);
            _autoColor.gameObject.SetActive(hasDecal);

            var autoColor = ((Decal)_boxedDecal).isAutoColor;
            _color.gameObject.SetActive(!autoColor && hasDecal);
            _smoothness.gameObject.SetActive(!autoColor && hasDecal);
        }
    }
}
