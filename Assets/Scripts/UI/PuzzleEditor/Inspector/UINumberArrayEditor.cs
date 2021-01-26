using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor    
{
    class UINumberArrayEditor : UIPropertyEditor, IInspectorStateProvider
    {
        [SerializeField] private GameObject _itemPrefab = null;
        [SerializeField] private UIList _items = null;

        private List<int> _values;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            _items.transform.DetachAndDestroyChildren();
            _items.onReorderItem += OnReorderItem;

            _values = target.GetValue<int[]>()?.ToList() ?? new List<int>();
            foreach (var value in _values)
                AddValue(value);

            label = target.name;

            _items.gameObject.SetActive(_values.Count > 0);
        }

        private void OnReorderItem(int from, int to)
        {
            var step = _values[from];
            _values.RemoveAt(from);
            _values.Insert(to, step);
            UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _values.ToArray()), false, (command) => {
                _items.Select(to);
            });
        }

        public void OnAddButton()
        {
            _values.Add(1);
            AddValue(1);
            _items.Select(_items.itemCount - 1);
            UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _values.ToArray()));
        }

        private void RemoveValue(int index)
        {
            _values.RemoveAt(index);

            UIPuzzleEditor.ExecuteCommand(
                new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _values.ToArray()), false, (command) => {
                    _items.Select(Mathf.Min(index, _items.itemCount - 1));
                });
        }

        private UINumberArrayEditorItem AddValue(int value)
        {
            var editor = Instantiate(_itemPrefab, _items.transform).GetComponent<UINumberArrayEditorItem>();
            editor.onValueChanged += (v) => {
                _values[editor.transform.GetSiblingIndex()] = v;
                UIPuzzleEditor.ExecuteCommand(
                    new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _values.ToArray()), false, (command) => {
                        _items.Select(Mathf.Min(_items.selected, _items.itemCount - 1));
                    });
            };
            editor.onDeleted += (item) => RemoveValue(item.transform.GetSiblingIndex());
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
                var editor = inspector.GetComponentsInChildren<UINumberArrayEditor>().FirstOrDefault();
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
