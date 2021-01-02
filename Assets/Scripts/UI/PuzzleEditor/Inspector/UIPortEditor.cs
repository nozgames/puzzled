using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIPortEditor : UIPropertyEditor, IInspectorStateProvider
    {
        [SerializeField] private Transform _wires = null;
        [SerializeField] private GameObject _wirePrefab = null;
        [SerializeField] private bool _reorderable = false;
        [SerializeField] private GameObject _content = null;
        [SerializeField] private GameObject _empty = null;
        [SerializeField] private Button _moveUpButton = null;
        [SerializeField] private Button _moveDownButton = null;
        [SerializeField] private Button _deleteButton = null;
        [SerializeField] private UIList _list = null;

        [SerializeField] private UISequence _sequence = null;

        private Port _port;

        private List<Wire> wires => _port.wires;

        public bool isReorderable => _reorderable;

        public UISequence sequence => _sequence;

        public Port port => _port;

        private void Awake()
        {
            _list.onSelectionChanged += OnSelectionChanged;

            _deleteButton.onClick.AddListener(OnDeleteButton);
            _moveUpButton.onClick.AddListener(OnMoveUpButton);
            _moveDownButton.onClick.AddListener(OnMoveDownButton);

            if(_sequence != null)
                _sequence.onSelectionChanged += OnSequenceSelectionChanged;
        }

        private void OnSequenceSelectionChanged(int selection)
        {
            if (selection == -1)
                return;

            UpdateLabel();
            UpdateWires();
        }

        private void UpdateLabel()
        {
            if (_sequence != null)
                label = $"{target.name} ({_sequence.GetStepName(_sequence.selection)})";
            else
                label = target.name;
        }

        private void OnSelectionChanged(int obj)
        {
            UpdateButtons();
        }

        protected override void OnTargetChanged()
        {
            _port = target.tileProperty.GetValue<Port>(target.tile);

            _wires.DetachAndDestroyChildren();

            foreach (var wire in wires)
                Instantiate(_wirePrefab, _wires).GetComponent<UIWireEditor>().wire = wire;

            _empty.SetActive(_wires.childCount <= 0);
            _content.SetActive(_wires.childCount > 0);

            if (_sequence != null)
                _sequence.tile = target.tile;

            UpdateLabel();
            UpdateWires();
            UpdateButtons();

            base.OnTargetChanged();
        }

        public void UpdateWires()
        {
            for (int i = 0; i < _wires.childCount; i++)
            {
                _wires.GetChild(i).GetComponent<UIWireEditor>().UpdateState();
            }
        }

        public void Select(int index)
        {
            if (_wires.childCount == 0)
                return;

            _wires.GetChild(Mathf.Clamp(index, 0, _wires.childCount - 1)).GetComponent<UIWireEditor>().Select();
        }

        public void OnMoveUpButton()
        {
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.WireReorderCommand(_port.wires, _list.selected, _list.selected - 1));
        }

        public void OnMoveDownButton()
        {
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.WireReorderCommand(_port.wires, _list.selected, _list.selected + 1));
        }

        private UIWireEditor GetWireEditor(int index) =>
            _wires.GetChild(index).GetComponent<UIWireEditor>();

        public void OnDeleteButton()
        {
            var wireEditor = GetWireEditor(_list.selected);
            var wire = wireEditor.wire;
            if (_list.selected + 1 < _list.itemCount)
                GetWireEditor(_list.selected + 1).wire.selected = true;
            else if (_list.selected > 0)
                GetWireEditor(_list.selected - 1).wire.selected = true;

            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.WireDestroyCommand(wire));
        }

        private void UpdateButtons()
        {
            _deleteButton.interactable = _list.selected != -1 && _wires.childCount > 0;
            _moveUpButton.interactable = _list.selected > 0;
            _moveDownButton.interactable = _list.selected >= 0 && _list.selected < _list.itemCount - 1;
        }

        /// <summary>
        /// Saves the state of the wires editor for the next tile the tile is selected
        /// </summary>
        private class WiresState : IInspectorState
        {
            public Port port;
            //public Wire selectedWire;
            public int sequenceStep;

            public void Apply(Transform inspector)
            {
                var editor = inspector.GetComponentsInChildren<UIPortEditor>().FirstOrDefault(c => c._port == port);
                if (null == editor)
                    return;

                if (editor._sequence != null && sequenceStep > 0)
                    editor._sequence.selection = sequenceStep;

#if false
                if (selectedWire != null)
                {
                    UIPuzzleEditor.selectedWire = selectedWire;
                    editor._wires.GetComponentsInChildren<UIWireEditor>().FirstOrDefault(o => o.wire == selectedWire);
                }
#endif
            }
        }

        /// <summary>
        /// Return the inspector state 
        /// </summary>
        IInspectorState IInspectorStateProvider.GetState() =>
            new WiresState {
                port = _port,
                //selectedWire = wires.FirstOrDefault(w => w.selected),
                sequenceStep = _sequence != null ? _sequence.selection : -1
            };
    }
}
