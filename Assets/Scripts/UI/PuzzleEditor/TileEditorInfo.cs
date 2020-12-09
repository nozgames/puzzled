using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Puzzled
{
    public class TileEditorInfo : MonoBehaviour
    {
        public class EditableProperty
        {
            public TileComponent component;
            public PropertyInfo property;
            public EditableAttribute editable;

            public void SetValue (string value)
            {
                if (property.PropertyType == typeof(int))
                    property.SetValue(component, int.TryParse(value, out var parsed) ? parsed : 0);
                else if (property.PropertyType == typeof(bool))
                    property.SetValue(component, bool.TryParse(value, out var parsed) ? parsed : false);
                else if (property.PropertyType == typeof(string))
                    property.SetValue(component, value);
                else if (property.PropertyType == typeof(Guid))
                    property.SetValue(component, Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty);
            }

            public string GetValue() => property.GetValue(component).ToString();
        }

        public EditableProperty[] editableProperties;

        public Guid guid { get; set; }

        private void Awake()
        {
            editableProperties = GetComponent<Tile>().GetComponentsInChildren<TileComponent>()
                .SelectMany(tc =>
                    tc.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance),
                    (tc, p) => new EditableProperty { component = tc, property = p, editable = p.GetCustomAttribute<EditableAttribute>() })
                .Where(ep => ep.editable != null)
                .ToArray();
        }

        public void SetEditableProperty(string name, string value)
        {
            var property = editableProperties
                .Where(ep => string.Compare(ep.property.Name, name, true) == 0)
                .FirstOrDefault();

            if (null == property)
                return;

            property.SetValue(value);
        }
    }
}
