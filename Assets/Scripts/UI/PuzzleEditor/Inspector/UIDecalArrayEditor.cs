using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled.Editor    
{
    class UIDecalArrayEditor : UIPropertyEditor, IInspectorStateProvider
    {
        [SerializeField] private GameObject _itemPrefab = null;
        [SerializeField] private UIList _items = null;

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

            _decals = target.GetValue<Decal[]>()?.ToList() ?? new List<Decal>();
            foreach (var decal in _decals)
                AddDecal(decal);

            _items.gameObject.SetActive(_decals.Count > 0);
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
