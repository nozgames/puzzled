using System.Reflection;
using Puzzled.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    public class UIEditWorldPropertiesScreen : UIScreen
    {
        private class WorldPropertyTarget : IPropertyEditorTarget
        {
            private World _world;
            private PropertyInfo _property;
            private System.Action<bool> _onValueChanged;

            public string id => _property.Name;

            public string name => _property.Name;

            public string placeholder { get; set; }

            public Vector2Int range { get; set; } = Vector2Int.zero;

            public object GetValue() => _property.GetValue(_world);

            public T GetValue<T>() => (T)_property.GetValue(_world);

            public void SetValue(object value, bool commit = true)
            {
                _property.SetValue(_world, value);
                _onValueChanged?.Invoke(commit);
            }

            public WorldPropertyTarget(World world, string propertyName, System.Action<bool> onValueChanged)
            {
                _world = world;
                _property = world.GetType().GetProperty(propertyName);
                _onValueChanged = onValueChanged;
            }
        }

        [SerializeField] private Button _closeButton = null;
        [SerializeField] private UIBoolEditor _testBool = null;

        private World _world;

        public World world {
            get => _world;
            set {
                _world = value;
                UpdateWorld();
            }
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => {
                UIManager.ShowEditWorldScreen();
            });
        }

        private void OnEnable()
        {
            UpdateWorld();
        }

        private void UpdateWorld()
        {
            if (null == _world || !isActiveAndEnabled)
                return;

            _testBool.target = new WorldPropertyTarget(_world, "test", null);
        }
    }
}
