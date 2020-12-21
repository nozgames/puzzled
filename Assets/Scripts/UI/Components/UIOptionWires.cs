using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIOptionWires : UIOptionEditor
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

            var wires = _input ? tile.inputs : tile.outputs;
            foreach (var wire in wires)
            {
                Instantiate(_wirePrefab, _wires).GetComponent<UIOptionWire>().target = wire;
            }

            _empty.SetActive(wires.Count <= 0);
            _content.SetActive(wires.Count > 0);

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

            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.ReorderWireCommand(isInput ? wire.to.tile.inputs : wire.from.tile.outputs, index, index - 1));

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

            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.ReorderWireCommand(isInput ? wire.to.tile.inputs : wire.from.tile.outputs, index, index + 1));

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

        public void OnDeleteButton()
        {
            var wireEditor = _wires.GetChild(_list.selected).GetComponent<UIOptionWire>();
            var wire = (Wire)wireEditor.target;
            UIPuzzleEditor.ExecuteCommand(new Editor.Commands.DestroyWireCommand(wire));
        }

        private void UpdateButtons()
        {
            _deleteButton.interactable = _list.selected != -1 && _wires.childCount > 0;
            _moveUpButton.interactable = _list.selected > 0;
            _moveDownButton.interactable = _list.selected >= 0 && _list.selected < _list.itemCount - 1;
        }
    }
}
