using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIOptionWires : UIOptionEditor, IInspectorStateProvider
    {
        [SerializeField] private Transform _wires = null;
        [SerializeField] private GameObject _wirePrefab = null;
        [SerializeField] private bool _input = true;
        [SerializeField] private bool _reorderable = false;
        [SerializeField] private GameObject _content = null;
        [SerializeField] private GameObject _empty = null;
        [SerializeField] private Button _moveUpButton = null;
        [SerializeField] private Button _moveDownButton = null;
        [SerializeField] private Button _deleteButton = null;
        [SerializeField] private UIList _list = null;

        [SerializeField] private UISequence _sequence = null;

        public bool isInput => _input;
        public bool isReorderable => _reorderable;

        public UISequence sequence => _sequence;

        private List<Wire> wires => _input ? ((Tile)target).inputs : ((Tile)target).outputs;

        private void Awake()
        {
            _list.onSelectionChanged += OnSelectionChanged;
            
            if(_sequence != null)
            {
                _sequence.onSelectionChanged += OnSequenceSelectionChanged;
            }
        }

        private void OnSequenceSelectionChanged(int selection)
        {
            if (selection == -1)
                return;

            if (_sequence != null)
                label = $"{(isInput ? "Inputs" : "Outputs")} ({_sequence.GetStepName(selection)})";

            UpdateWires();
        }

        private void OnSelectionChanged(int obj)
        {
            UpdateButtons();
        }

        protected override void OnTargetChanged(object target)
        {
            var tile = (Tile)target;

            _wires.DetachAndDestroyChildren();

            foreach (var wire in wires)
                Instantiate(_wirePrefab, _wires).GetComponent<UIOptionWire>().target = wire;

            _empty.SetActive(_wires.childCount <= 0);
            _content.SetActive(_wires.childCount > 0);

            if (_sequence != null)
                _sequence.tile = tile;

            UpdateWires();
            UpdateButtons();

            base.OnTargetChanged(target);
        }

        public void UpdateWires()
        {
            for (int i = 0; i < _wires.childCount; i++)
            {
                _wires.GetChild(i).GetComponent<UIOptionWire>().UpdateState();
            }
        }

        public void Select(int index)
        {
            if (_wires.childCount == 0)
                return;

            _wires.GetChild(Mathf.Clamp(index, 0, _wires.childCount - 1)).GetComponent<UIOptionWire>().Select();
        }

        public void OnMoveUpButton()
        {
            var index = _list.selected;
            var wireEditor = _wires.GetChild(index).GetComponent<UIOptionWire>();
            var wire = (Wire)wireEditor.target;
            if (index == 0)
                return;

            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.WireReorderCommand(isInput ? wire.to.tile.inputs : wire.from.tile.outputs, index, index - 1));

#if false
            if (isInput)
                wire.to.tile.SetInputIndex(wire, index - 1);
            else
                wire.from.tile.SetOutputIndex(wire, index - 1);
#endif

            wireEditor.transform.SetSiblingIndex(index - 1);
            _list.Select(index - 1);

            UpdateWires();
            UpdateButtons();
        }

        public void OnMoveDownButton()
        {
            var index = _list.selected;
            var wireEditor = _wires.GetChild(index).GetComponent<UIOptionWire>();
            var wire = (Wire)wireEditor.target;

            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.WireReorderCommand(isInput ? wire.to.tile.inputs : wire.from.tile.outputs, index, index + 1));

#if false
            if (isInput)
            {
                if (index >= wire.to.tile.inputCount - 1)
                    return;

                wire.to.tile.SetInputIndex(wire, index + 1);
            } else
            {
                if (index >= wire.from.tile.outputCount - 1)
                    return;

                wire.from.tile.SetOutputIndex(wire, index + 1);
            }
#endif

            wireEditor.transform.SetSiblingIndex(index + 1);
            _list.Select(index + 1);

            UpdateWires();
            UpdateButtons();
        }

        private UIOptionWire GetWireEditor(int index) =>
            _wires.GetChild(index).GetComponent<UIOptionWire>();

        public void OnDeleteButton()
        {
            var wireEditor = GetWireEditor(_list.selected);
            var wire = (Wire)wireEditor.target;
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
            public bool isInput;
            public Wire selectedWire;
            public int sequenceStep;

            public void Apply(Transform inspector)
            {
                var editor = inspector.GetComponentsInChildren<UIOptionWires>().FirstOrDefault(c => c.isInput == isInput);
                if (null == editor)
                    return;

                if (editor._sequence != null && sequenceStep > 0)
                    editor._sequence.selection = sequenceStep;

                if (selectedWire != null)
                {
                    UIPuzzleEditor.selectedWire = selectedWire;
                    editor._wires.GetComponentsInChildren<UIOptionWire>().FirstOrDefault(o => o.wire == selectedWire);
                }
            }
        }

        /// <summary>
        /// Return the inspector state 
        /// </summary>
        IInspectorState IInspectorStateProvider.GetState() =>
            new WiresState {
                isInput = isInput,
                selectedWire = wires.FirstOrDefault(w => w.selected),
                sequenceStep = _sequence != null ? _sequence.selection : -1
            };
    }
}
