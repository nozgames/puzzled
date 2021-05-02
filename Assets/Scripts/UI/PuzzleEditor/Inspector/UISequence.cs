using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Puzzled.UI;

namespace Puzzled.Editor
{
    public class UISequence : MonoBehaviour
    {
        [SerializeField] private UIList _list = null;
        [SerializeField] private GameObject _stepPrefab = null;

        public event Action<int> onSelectionChanged;
        public event Action<int,Editor.Commands.GroupCommand> onStepRemoved;
        public event Action<int,int,Editor.Commands.GroupCommand> onStepMoved;

        private Tile _tile;
        private List<string> _steps;

        public int selection {
            get => _list.selected;
            set => _list.Select(value);
        }

        public Tile tile {
            get => _tile;
            set {
                _tile = value;
                if (null == _tile)
                    return;

                _steps = _tile.GetPropertyValue<string[]>("steps")?.ToList() ?? new List<string>();
                if (_steps.Count == 0)
                {
                    OnAddButton();
                    return;
                }

                foreach (var stepText in _steps)
                {
                    var step = Instantiate(_stepPrefab, _list.transform).GetComponent<UISequenceStep>();
                    step.text = stepText;
                    step.onNameChanged += OnStepNameChanged;
                    step.onDeleted += RemoveState;
                }

                _list.Select(0);
            }
        }

        public string GetStepName(int step) => _steps[step];

        private void Awake()
        {
            _list.onSelectionChanged += OnSelectionChanged;
            _list.onReorderItem += OnReorderState;
        }

        private void OnReorderState(int from, int to)
        {
            var group = new Editor.Commands.GroupCommand();
            var step = _steps[from];
            _steps.RemoveAt(from);
            _steps.Insert(to, step);
            group.Add(new Editor.Commands.TileSetPropertyCommand(_tile, "steps", _steps.ToArray()));

            onStepMoved?.Invoke(from, to, group);

            UIPuzzleEditor.ExecuteCommand(group, false, (c) => {
                _list.Select(to);
            });
        }

        private void OnEnable()
        {            
        }

        private void OnSelectionChanged(int selection)
        {
            onSelectionChanged?.Invoke(selection);
        }

        public void OnAddButton()
        {
            var pageCount = _list.transform.childCount + 1;
            if (pageCount >= 32)
                return;
            
            _steps.Add("New Step");
            var step = Instantiate(_stepPrefab, _list.transform).GetComponent<UISequenceStep>();
            _list.Select(_list.itemCount - 1);
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(_tile, "steps", _steps.ToArray()));
        }

        private void OnStepNameChanged(UISequenceStep step)
        {
            _steps[step.transform.GetSiblingIndex()] = step.text;
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(_tile, "steps", _steps.ToArray()));
        }

        private void RemoveState(UISequenceStep state)
        {
            var group = new Editor.Commands.GroupCommand();
            var index = state.transform.GetSiblingIndex();
            _steps.RemoveAt(index);
            group.Add(new Editor.Commands.TileSetPropertyCommand(_tile, "steps", _steps.ToArray()));

            onStepRemoved?.Invoke(index, group);

            UIPuzzleEditor.ExecuteCommand(group, false, (c) => {
                _list.Select(Mathf.Min(index, _steps.Count - 1));
            });
        }
    }
}
