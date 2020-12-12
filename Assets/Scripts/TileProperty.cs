using System;
using System.Linq;
using System.Reflection;

namespace Puzzled
{
    public class TileProperty
    {
        public PropertyInfo property;
        public EditableAttribute editable;

        public void SetValue(Tile tile, string value)
        {
            var component = tile.GetComponentInChildren(property.DeclaringType);
            if (property.PropertyType == typeof(int))
                property.SetValue(component, int.TryParse(value, out var parsed) ? parsed : 0);
            else if (property.PropertyType == typeof(bool))
                property.SetValue(component, bool.TryParse(value, out var parsed) ? parsed : false);
            else if (property.PropertyType == typeof(string))
                property.SetValue(component, value);
            else if (property.PropertyType == typeof(Guid))
                property.SetValue(component, Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty);
        }

        public void SetValue(Tile tile, int value) => SetValue(tile, value.ToString());
        public void SetValue(Tile tile, bool value) => SetValue(tile, value.ToString());
        public void SetValue(Tile tile, Guid value) => SetValue(tile, value.ToString());

        public string GetValue(Tile tile) => property.GetValue(tile.GetComponentInChildren(property.DeclaringType)).ToString();
        public int GetValueInt(Tile tile) => int.TryParse(GetValue(tile), out var result) ? result : 0;
        public bool GetValueBool(Tile tile) => bool.TryParse(GetValue(tile), out var result) ? result : false;
        public Guid GetValueGuid(Tile tile) => Guid.TryParse(GetValue(tile), out var result) ? result : Guid.Empty;
    }
}
