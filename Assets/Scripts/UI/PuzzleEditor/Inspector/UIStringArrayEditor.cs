using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled.Editor    
{
    class UIStringArrayEditor : UIPropertyEditor, IInspectorStateProvider
    {
        [SerializeField] private GameObject _itemPrefab = null;
        [SerializeField] private UIList _items = null;

        private List<string> _values;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            _items.transform.DetachAndDestroyChildren();
            _items.onReorderItem += OnReorderItem;

            _values = target.GetValue<string[]>()?.ToList() ?? new List<string>();
            foreach (var value in _values)
                AddValue(value);

            _items.gameObject.SetActive(_values.Count > 0);

            label = target.name;
        }

        public void OnAddButton()
        {
            _values.Add("");
            AddValue("");
            _items.Select(_items.itemCount - 1);

            UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _values.ToArray()));
        }

        private void RemoveItem (UIStringArrayEditorItem item)
        {
            var index = item.transform.GetSiblingIndex();
            _values.RemoveAt(index);
            UIPuzzleEditor.ExecuteCommand(
                new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _values.ToArray()), false, (command) => {
                    _items.Select(Mathf.Min(index, _values.Count - 1));
                });
        }

        private void OnReorderItem(int from, int to)
        {
            var group = new Editor.Commands.GroupCommand();
            var step = _values[from];
            _values.RemoveAt(from);
            _values.Insert(to, step);
            UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _values.ToArray()), false, (command) => {
                _items.Select(to);
            });
        }

        private UIStringArrayEditorItem AddValue(string value)
        {
            var editor = Instantiate(_itemPrefab, _items.transform).GetComponent<UIStringArrayEditorItem>();
            editor.onValueChanged += (v) => {
                var option = ((TilePropertyEditorTarget)target);
                _values[editor.transform.GetSiblingIndex()] = v;
                UIPuzzleEditor.ExecuteCommand(
                    new Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, _values.ToArray()), false, (command) => {
                        _items.Select(Mathf.Min(_items.selected, _items.itemCount - 1));
                    });
            };
            editor.onDeleted += (v) => RemoveItem(v);
            editor.value = value;
            return editor;
        }

        /// <summary>
        /// Saves the list state in the inspector
        /// </summary>
        private class InspectorState : IInspectorState
        {
            public int selectedIndex;

            public void Apply(Transform inspector)
            {
                var editor = inspector.GetComponentsInChildren<UIStringArrayEditor>().FirstOrDefault();
                if (null == editor)
                    return;

                if (selectedIndex == -1)
                    editor._items.ClearSelection();
                else
                    editor._items.Select(selectedIndex);
            }
        }

        public IInspectorState GetState() => new InspectorState { selectedIndex = _items.selected };
    }
}
