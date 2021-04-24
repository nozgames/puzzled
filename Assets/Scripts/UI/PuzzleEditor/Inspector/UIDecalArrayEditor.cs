using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled.Editor    
{
    class UIDecalArrayEditor : UIPropertyEditor, IInspectorStateProvider
    {
        [SerializeField] private GameObject _itemPrefab = null;
        [SerializeField] private UIList _items = null;
        [SerializeField] private UIDecalEditor _decalEditor = null;

        private List<Decal> _decals;

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
            _items.onSelectionChanged += OnSelectionChanged;

            _decalEditor.gameObject.SetActive(false);

            _decals = target.GetValue<Decal[]>()?.ToList() ?? new List<Decal>();
            foreach (var decal in _decals)
                AddDecal(decal);

            _items.gameObject.SetActive(_decals.Count > 0);
        }

        private class PropertyTarget : IPropertyEditorTarget
        {
            private UIDecalArrayEditor _editor;

            public string id => throw new System.NotImplementedException();

            public string name => $"{_editor.target.name} {_editor._items.selected}";

            public string placeholder => null;

            public Vector2Int range => Vector2Int.zero;

            public object GetValue() => _editor._decals[_editor._items.selected];

            public T GetValue<T>() => (T)GetValue();

            public void SetValue(object value, bool commit = true)
            {
                _editor._decals[_editor._items.selected] = (Decal)value;
                _editor.target.SetValue(_editor._decals.ToArray(), commit);
            }

            public PropertyTarget (UIDecalArrayEditor editor)
            {
                _editor = editor;
            }
        }

        private void OnSelectionChanged(int selection)
        {
            if(selection < 0)
            {
                _decalEditor.gameObject.SetActive(false);
                return;
            }

            _decalEditor.gameObject.SetActive(true);
            _decalEditor.target = new PropertyTarget(this);
        }

        public void OnAddButton()
        {
            UIPuzzleEditor.instance.ChooseDecal(Decal.none, (decal) => {
                _decals.Add(decal);
                AddDecal(decal);
                _items.Select(_items.itemCount - 1);
                target.SetValue(_decals.ToArray());
            });
        }

        private void OnReorderItem(int from, int to)
        {
            var step = _decals[from];
            _decals.RemoveAt(from);
            _decals.Insert(to, step);
            _items.Select(to);
            target.SetValue(_decals.ToArray());
        }

        private void RemoveDecal(int index)
        {
            _decals.RemoveAt(_items.selected);
            _items.Select(Mathf.Min(_items.selected, _items.itemCount - 1));
            target.SetValue(_decals.ToArray());
        }

        private UIDecalArrayEditorItem AddDecal(Decal decal)
        {
            var editor = Instantiate(_itemPrefab, _items.transform).GetComponent<UIDecalArrayEditorItem>();
            editor.value = decal;
            editor.onValueChanged += (d) => {
                _decals[editor.transform.GetSiblingIndex()] = d;
                target.SetValue(_decals.ToArray());
            };
            editor.onDeleted += (item) => RemoveDecal(item.transform.GetSiblingIndex());
            return editor;
        }
    }
}
