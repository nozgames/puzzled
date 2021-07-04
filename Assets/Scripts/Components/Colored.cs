using System;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Colored : TileComponent
    {
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private string _propertyName = null;
        [SerializeField] private string _propertyDisplayName = null;

        private Color _defaultColor = Color.white;
        private bool _overrideColor = false;

        public Action<Color> onColorChanged;

        private void Awake()
        {
            _defaultColor = _color;
        }

        [Editable(dynamicName = "propertyName", dynamicDisplayName = "propertyDisplayName")]
        private bool overrideColor {
            get => _overrideColor;
            set {
                _overrideColor = value;
                onColorChanged?.Invoke(color);
            }
        }

        [Editable(hiddenIfFalse = "overrideColor", dynamicDisplayName = "hiddenDisplayName")]
        public Color color {
            get => overrideColor ? _color : _defaultColor;
            set {
                _color = value;
                onColorChanged?.Invoke(color);
            }
        }

        public string propertyName => _propertyName;
        public string propertyDisplayName => _propertyDisplayName;
        public string hiddenDisplayName => " ";
    }
}
