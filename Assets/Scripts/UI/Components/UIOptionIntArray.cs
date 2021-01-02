using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor    
{
    class UIOptionIntArray : UIPropertyEditor, IInspectorStateProvider
    {
        [SerializeField] private GameObject _itemPrefab = null;
        [SerializeField] private UIList _items = null;

        [SerializeField] private Button _moveUpButton = null;
        [SerializeField] private Button _moveDownButton = null;
        [SerializeField] private Button _removeButton = null;
        [SerializeField] private Button _addButton = null;

        private List<int> _values;

        protected override void OnTargetChanged()
        {
            base.OnTargetChanged();

            _items.transform.DetachAndDestroyChildren();
            _items.onSelectionChanged += (index) => UpdateButtons();

            _addButton.onClick.AddListener(OnAddButton);
            _removeButton.onClick.AddListener(OnRemoveButton);
            _moveUpButton.onClick.AddListener(OnMoveUpButton);
            _moveDownButton.onClick.AddListener(OnMoveDownButton);

            _values = target.GetValue<int[]>()?.ToList() ?? new List<int>();
            foreach (var value in _values)
                AddValue(value);

            label = target.name;

            UpdateButtons();
        }

        public void OnAddButton()
        {
            _values.Add(1);
            AddValue(1);
            _items.Select(_items.itemCount - 1);

            var option = ((TilePropertyEditorTarget)target);
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, _values.ToArray()));
        }

        public void OnRemoveButton()
        {
            _values.RemoveAt(_items.selected);

            var option = ((TilePropertyEditorTarget)target);
            UIPuzzleEditor.ExecuteCommand(
                new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, _values.ToArray()), false, (command) => {
                    _items.Select(Mathf.Min(_items.selected, _items.itemCount - 1));
                });
        }

        private UIOptionIntArrayItem AddValue(int value)
        {
            var editor = Instantiate(_itemPrefab, _items.transform).GetComponent<UIOptionIntArrayItem>();
            editor.onValueChanged += (v) => {
                var option = ((TilePropertyEditorTarget)target);
                _values[editor.transform.GetSiblingIndex()] = v;
                UIPuzzleEditor.ExecuteCommand(
                    new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, _values.ToArray()), false, (command) => {
                        _items.Select(Mathf.Min(_items.selected, _items.itemCount - 1));
                    });
            };
            editor.value = value;
            return editor;
        }

        private void OnMoveUpButton()
        {
            var option = ((TilePropertyEditorTarget)target);
            var temp = _values[_items.selected - 1];
            _values[_items.selected - 1] = _values[_items.selected];
            _values[_items.selected] = temp;
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, _values.ToArray()), false, (cmd) => {
                _items.Select(_items.selected - 1);
            });
        }

        private void OnMoveDownButton()
        {
            var option = ((TilePropertyEditorTarget)target);
            var temp = _values[_items.selected + 1];
            _values[_items.selected + 1] = _values[_items.selected];
            _values[_items.selected] = temp;
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(option.tile, option.tileProperty.name, _values.ToArray()), false, (cmd) => {
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

            public void Apply(Transform inspector)
            {
                var editor = inspector.GetComponentsInChildren<UIOptionIntArray>().FirstOrDefault();
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
