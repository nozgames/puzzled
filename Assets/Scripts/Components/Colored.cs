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

        public Action<Color> onColorChanged;

        [Editable(dynamicName = "propertyName", dynamicDisplayName = "propertyDisplayName")]
        public Color color {
            get => _color;
            set {
                _color = value;
                onColorChanged?.Invoke(_color);
            }
        }

        public string propertyName => _propertyName;
        public string propertyDisplayName => _propertyDisplayName;
    }
}
