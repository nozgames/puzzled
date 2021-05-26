using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Puzzled.UI;

namespace Puzzled.Editor    
{
    class UINumberArrayEditor : UIPropertyEditor, IInspectorStateProvider
    {
        [SerializeField] private GameObject _itemPrefab = null;
        [SerializeField] private UIList _items = null;

        private List<int> _values;

        public object inspectorState {
            get => _items.selected;
            set {
                var index = (int)value;
                if (index == -1)
                    _items.ClearSelection();
                else
                    _items.SelectItem(index);
            }
        }

        public string inspectorStateId => target.id;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            _items.transform.DetachAndDestroyChildren();
            _items.onReorderItem += OnReorderItem;

            _values = target.GetValue<int[]>()?.ToList() ?? new List<int>();
            foreach (var value in _values)
                AddValue(value);            

            _items.gameObject.SetActive(_values.Count > 0);
        }

        private void OnReorderItem(int from, int to)
        {
            var step = _values[from];
            _values.RemoveAt(from);
            _values.Insert(to, step);
            target.SetValue(_values.ToArray());
        }

        public void OnAddButton()
        {
            _values.Add(1);
            AddValue(1);
            _items.SelectItem(_items.itemCount - 1);
            target.SetValue(_values.ToArray());
        }

        private void RemoveValue(int index)
        {
            _values.RemoveAt(index);
            target.SetValue(_values.ToArray());
        }

        private UINumberArrayEditorItem AddValue(int value)
        {
            var editor = Instantiate(_itemPrefab, _items.transform).GetComponent<UINumberArrayEditorItem>();
            editor.onValueChanged += (v) => {
                _values[editor.transform.GetSiblingIndex()] = v;
                target.SetValue(_values.ToArray());
            };
            editor.onDeleted += (item) => RemoveValue(item.transform.GetSiblingIndex());
            editor.value = value;
            return editor;
        }
    }
}
