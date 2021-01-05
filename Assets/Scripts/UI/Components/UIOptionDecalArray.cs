using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor    
{
    class UIOptionDecalArray : UIPropertyEditor, IInspectorStateProvider
    {
        [SerializeField] private GameObject _itemPrefab = null;
        [SerializeField] private UIList _items = null;

        [SerializeField] private Button _moveUpButton = null;
        [SerializeField] private Button _moveDownButton = null;
        [SerializeField] private Button _removeButton = null;
        [SerializeField] private Button _addButton = null;

        private List<Decal> _decals;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            label = target.name;

            _items.transform.DetachAndDestroyChildren();
            _items.onSelectionChanged += (index) => UpdateButtons();

            _addButton.onClick.AddListener(OnAddButton);
            _removeButton.onClick.AddListener(OnRemoveButton);
            _moveUpButton.onClick.AddListener(OnMoveUpButton);
            _moveDownButton.onClick.AddListener(OnMoveDownButton);

            _decals = target.GetValue<Decal[]>()?.ToList() ?? new List<Decal>();
            foreach (var decal in _decals)
                AddDecal(decal);

            UpdateButtons();
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

        public void OnRemoveButton()
        {
            _decals.RemoveAt(_items.selected);

            UIPuzzleEditor.ExecuteCommand(
                new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _decals.ToArray()), false, (command) => {
                    _items.Select(Mathf.Min(_items.selected, _items.itemCount - 1));
                });
        }

        private UIDecalEditor AddDecal(Decal decal)
        {
            var editor = Instantiate(_itemPrefab, _items.transform).GetComponent<UIDecalEditor>();
            editor.decal = decal;
            editor.onDecalChanged += (d) => {
                _decals[editor.transform.GetSiblingIndex()] = d;
                UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _decals.ToArray()));
            };
            return editor;
        }

        private void OnMoveUpButton()
        {
            var temp = _decals[_items.selected - 1];
           _decals[_items.selected - 1] = _decals[_items.selected];
            _decals[_items.selected] = temp;
            UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _decals.ToArray()), false, (cmd) => {
                _items.Select(_items.selected - 1);
            });
        }

        private void OnMoveDownButton()
        {
            var temp = _decals[_items.selected + 1];
            _decals[_items.selected + 1] = _decals[_items.selected];
            _decals[_items.selected] = temp;
            UIPuzzleEditor.ExecuteCommand(new Commands.TileSetPropertyCommand(target.tile, target.tileProperty.name, _decals.ToArray()), false, (cmd) => {
                _items.Select(_items.selected + 1);
            });
        }

        private void UpdateButtons()
        {
            _moveDownButton.interactable = _items.selected >= 0 && _items.selected < _items.itemCount - 1;
            _moveUpButton.interactable = _items.selected > 0;
            _addButton.interactable = _items.itemCount < 64;
            _removeButton.interactable = _items.selected != -1;
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
                var editor = inspector.GetComponentsInChildren<UIOptionDecalArray>().Where(e => e.target.tileProperty == property).FirstOrDefault();
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
