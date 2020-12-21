using System;
using System.Linq;
using System.Reflection;

namespace Puzzled
{
    public class TileProperty
    {
        public PropertyInfo property;
        public EditableAttribute editable;

        private TileComponent GetComponent(Tile tile) => (TileComponent)tile.GetComponentInChildren(property.DeclaringType);

        public void SetValue(Tile tile, string value)
        {
            var component = GetComponent(tile);
            if (property.PropertyType == typeof(int))
                property.SetValue(component, int.TryParse(value, out var parsed) ? parsed : 0);
            else if (property.PropertyType == typeof(bool))
                property.SetValue(component, bool.TryParse(value, out var parsed) ? parsed : false);
            else if (property.PropertyType == typeof(string))
                property.SetValue(component, value);
            else if (property.PropertyType == typeof(Guid))
                property.SetValue(component, Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty);
            else if (property.PropertyType == typeof(string[]))
                property.SetValue(component, value.Split(','));
            else if (property.PropertyType == typeof(Decal))
                property.SetValue(component, DecalDatabase.GetDecal(Guid.TryParse(value, out var guid) ? guid : Guid.Empty));
        }

        public void SetValue(Tile tile, object value) => property.SetValue(GetComponent(tile), value);

        public string GetValue(Tile tile)
        {
            var value = GetValueObject(tile);
            if (property.PropertyType == typeof(string[]))
                return value != null ? string.Join(",", (string[])value) : "";
            else if (property.PropertyType == typeof(Decal))
            {
                var decal = (Decal)value;
                if (null == decal || decal.guid == Guid.Empty)
                    return "";

                return decal.guid.ToString();
            }

            return value.ToString();
        }

        private object GetValueObject(Tile tile) => property.GetValue(tile.GetComponentInChildren(property.DeclaringType));

        public int GetValueInt(Tile tile) => int.TryParse(GetValue(tile), out var result) ? result : 0;
        public bool GetValueBool(Tile tile) => bool.TryParse(GetValue(tile), out var result) ? result : false;
        public Guid GetValueGuid(Tile tile) => Guid.TryParse(GetValue(tile), out var result) ? result : Guid.Empty;
        public string[] GetValueStringArray(Tile tile)
        {
            var value = GetValue(tile);
            if (string.IsNullOrEmpty(value))
                return new string[] { };

            return value.Split(',');
        }
        public Decal GetValueDecal(Tile tile) => (Decal)GetValueObject(tile);
    }
}
