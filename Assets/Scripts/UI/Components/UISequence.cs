using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UISequence : MonoBehaviour
    {
        [SerializeField] private UIList _list = null;
        [SerializeField] private GameObject _stepPrefab = null;

        [SerializeField] private Button _moveUpButton = null;
        [SerializeField] private Button _moveDownButton = null;
        [SerializeField] private Button _removeButton = null;
        [SerializeField] private Button _addButton = null;

        public event Action<int> onSelectionChanged;
        public event Action<int> onStepRemoved;
        public event Action<int,int> onStepMoved;

        private Tile _tile;
        private List<string> _steps;

        public int selection => _list.selected;

        public Tile tile {
            get => _tile;
            set {
                _tile = value;
                if (null == _tile)
                    return;

                _steps = _tile.GetPropertyStringArray("steps").ToList();
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
                }

                _list.Select(0);

                UpdateButtons();
            }
        }

        public string GetStepName(int step) => _steps[step];

        private void Awake()
        {
            _list.onSelectionChanged += OnSelectionChanged;
        }

        private void OnEnable()
        {            
        }

        private void OnSelectionChanged(int selection)
        {
            onSelectionChanged?.Invoke(selection);
            UpdateButtons();
        }

        public void OnMoveUpButton()
        {
            if (_list.selected <= 0)
                return;

            var selection = _list.selected;
            var step = _steps[selection];
            _steps.RemoveAt(selection);
            _steps.Insert(selection - 1, step);
            _tile.SetProperty("steps", _steps.ToArray());

            onStepMoved?.Invoke(selection, selection - 1);

            _list.Select(_list.selected - 1);
        }

        public void OnMoveDownButton()
        {
            if (_list.selected >= _list.itemCount - 1)
                return;

            var selection = _list.selected;
            var step = _steps[selection];
            _steps.RemoveAt(selection);
            _steps.Insert(selection + 1, step);
            _tile.SetProperty("steps", _steps.ToArray());

            onStepMoved?.Invoke(selection, selection + 1);

            _list.Select(_list.selected + 1);
        }

        public void OnAddButton()
        {
            var pageCount = _list.transform.childCount + 1;
            if (pageCount >= 32)
                return;
            
            _steps.Add("New Step");
            _tile.SetProperty("steps", _steps.ToArray());

            var step = Instantiate(_stepPrefab, _list.transform).GetComponent<UISequenceStep>();
            step.text = "New Step";
            step.onNameChanged += OnStepNameChanged;

            _list.Select(_list.itemCount - 1);
        }

        private void OnStepNameChanged(UISequenceStep step)
        {
            _steps[step.transform.GetSiblingIndex()] = step.text;
            _tile.SetProperty("steps", _steps.ToArray());
        }

        public void OnRemoveButton()
        {
            _steps.RemoveAt(_list.selected);
            _tile.SetProperty("steps", _steps.ToArray());

            var selected = _list.selected;
            onStepRemoved?.Invoke(selected);

            var itemObject = _list.GetItem(_list.selected).gameObject;
            itemObject.transform.SetParent(null);
            Destroy(itemObject);

            _list.ClearSelection();
            _list.Select(Mathf.Min(selected,_list.itemCount-1));
        }

        private void UpdateButtons()
        {
            _moveDownButton.interactable = _list.selected >= 0 && _list.selected < _list.itemCount - 1;
            _moveUpButton.interactable = _list.selected > 0;
            _addButton.interactable = _list.itemCount < 32;
            _removeButton.interactable = _list.selected != -1;
        }
    }
}
