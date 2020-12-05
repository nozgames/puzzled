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
        [SerializeField] private ToggleGroup _toggleGroup = null;
        [SerializeField] private Button _moveUpButtom = null;
        [SerializeField] private Button _moveDownButton = null;
        [SerializeField] private Button _deleteButton = null;

        private UIOptionWire _selected = null;

        public bool isInput => _input;
        public bool isReorderable => _reorderable;

        private void OnEnable()
        {
            _moveUpButtom.gameObject.SetActive(_reorderable);
            _moveDownButton.gameObject.SetActive(_reorderable);
        }

        protected override void OnTargetChanged(object target)
        {
            var tile = (Tile)target;

            label = _input ? "Inputs" : "Outputs";

            _wires.DetachAndDestroyChildren();

            var wires = _input ? tile.inputs : tile.outputs;
            foreach (var wire in wires)
            {
                Instantiate(_wirePrefab, _wires).GetComponent<UIOptionWire>().target = wire;
            }

            OnSelectionChanged(null);
        }

        public void OnMoveUpButton()
        {
            if (null == _selected)
                return;

            var wire = ((Wire)_selected.target);
            var index = _input ? wire.to.tile.GetInputIndex(wire) : wire.from.tile.GetOutputIndex(wire);
            if (index == 0)
                return;

            if(_input)
                wire.to.tile.SetInputIndex(wire, index - 1);
            else
                wire.from.tile.SetOutputIndex(wire, index - 1);

            _selected.transform.SetSiblingIndex(index - 1);

            UpdateWires();
            UpdateButtons();
        }

        public void OnMoveDownButton()
        {
            if (null == _selected)
                return;

            if (null == _selected)
                return;

            var wire = ((Wire)_selected.target);
            if (_input)
            {
                var index = wire.to.tile.GetInputIndex(wire);
                if (index >= wire.to.tile.inputCount - 1)
                    return;

                wire.to.tile.SetInputIndex(wire, index + 1);
            }
            else
            {
                var index = wire.from.tile.GetOutputIndex(wire);
                if (index >= wire.from.tile.outputCount - 1)
                    return;

                wire.from.tile.SetOutputIndex(wire, index + 1);
            }

            _selected.transform.SetSiblingIndex(_selected.transform.GetSiblingIndex() + 1);

            UpdateWires();
            UpdateButtons();
        }

        public void OnDeleteButton()
        {
            if (_selected == null)
                return;

            // Destroy the wire
            Destroy(((Wire)_selected.target).gameObject);

            // Destroy the wire entry
            Destroy(_selected.gameObject);

            // No selection
            OnSelectionChanged(null);
        }

        internal void OnSelectionChanged(UIOptionWire wireOption)
        {
            _selected = wireOption;
            UpdateButtons();
        }

        private void UpdateWires()
        {
            for (int i = 0; i < _wires.childCount; i++)
                _wires.GetChild(i).GetComponent<UIOptionWire>().UpdateIndex();
        }

        private void UpdateButtons()
        {
            _moveUpButtom.interactable = _selected != null && _selected.transform.GetSiblingIndex() > 0;
            _moveDownButton.interactable = _selected != null && _selected.transform.GetSiblingIndex() < _selected.transform.parent.childCount - 1;
            _deleteButton.interactable = _selected != null;
        }
    }
}
