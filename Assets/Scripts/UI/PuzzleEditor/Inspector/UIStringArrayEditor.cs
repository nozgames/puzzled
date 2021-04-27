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

        public object inspectorState {
            get => _items.selected;
            set {
                var index = (int)value;
                if (index == -1)
                    _items.ClearSelection();
                else
                    _items.Select(index);
            }
        }

        public string inspectorStateId => target.id;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            _items.transform.DetachAndDestroyChildren();
            _items.onReorderItem += OnReorderItem;

            _values = target.GetValue<string[]>()?.ToList() ?? new List<string>();
            foreach (var value in _values)
                AddValue(value);

            _items.gameObject.SetActive(_values.Count > 0);
        }

        public void OnAddButton()
        {
            _values.Add("");
            AddValue("");
            _items.Select(_items.itemCount - 1);
            target.SetValue(_values.ToArray());
        }

        private void RemoveItem (UIStringArrayEditorItem item)
        {
            var index = item.transform.GetSiblingIndex();
            _values.RemoveAt(index);
            _items.Select(Mathf.Min(index, _values.Count - 1));
            target.SetValue(_values.ToArray());
        }

        private void OnReorderItem(int from, int to)
        {
            var step = _values[from];
            _values.RemoveAt(from);
            _values.Insert(to, step);
            target.SetValue(_values.ToArray());
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
    }
}
