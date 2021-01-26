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

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            label = target.name;

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
                UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _decals.ToArray()), false, (cmd) => {
                    AddDecal(decal);
                    _items.Select(_items.itemCount - 1);
                });
            });
        }

        private void OnReorderItem(int from, int to)
        {
            var step = _decals[from];
            _decals.RemoveAt(from);
            _decals.Insert(to, step);
            UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _decals.ToArray()), false, (command) => {
                _items.Select(to);
            });
        }

        private void RemoveDecal(int index)
        {
            _decals.RemoveAt(_items.selected);

            UIPuzzleEditor.ExecuteCommand(
                new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _decals.ToArray()), false, (command) => {
                    _items.Select(Mathf.Min(_items.selected, _items.itemCount - 1));
                });
        }

        private UIDecalArrayEditorItem AddDecal(Decal decal)
        {
            var editor = Instantiate(_itemPrefab, _items.transform).GetComponent<UIDecalArrayEditorItem>();
            editor.value = decal;
            editor.onValueChanged += (d) => {
                _decals[editor.transform.GetSiblingIndex()] = d;
                UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _decals.ToArray()));
            };
            editor.onDeleted += (item) => RemoveDecal(item.transform.GetSiblingIndex());
            return editor;
        }

        /// <summary>
        /// Saves the list state in the inspector
        /// </summary>
        private class InspectorState : IInspectorState
        {
            public int selectedIndex;
            public TileProperty property;

            public void Apply(Transform inspector)
            {
                var editor = inspector.GetComponentsInChildren<UIDecalArrayEditor>().Where(e => e.target.tileProperty == property).FirstOrDefault();
                if (null == editor)
                    return;

                if (selectedIndex == -1)
                    editor._items.ClearSelection();
                else
                    editor._items.Select(selectedIndex);
            }
        }

        public IInspectorState GetState() => new InspectorState { 
            selectedIndex = _items.selected,
            property = target.tileProperty 
        };
    }
}
