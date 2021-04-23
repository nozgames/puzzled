using System;
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
        [SerializeField] private UIList _list = null;
        [SerializeField] private Image _portIcon = null;

        [SerializeField] private UISequence _sequence = null;

        private Port _port;

        private List<Wire> wires => _port.wires;

        public bool isReorderable => _reorderable;

        public UISequence sequence => _sequence;

        public Port port => _port;

        public string inspectorStateId => target.id;

        protected override string label => _sequence != null ? $"{target.name} ({_sequence.GetStepName(_sequence.selection)})" : target.name;


        public object inspectorState {
            get => _sequence != null ? _sequence.selection : -1;
            set {
                var selection = (int)value;

                if (_sequence != null && selection > 0)
                    _sequence.selection = selection;
            }
        }

        private void Awake()
        {
            _list.onReorderItem += OnReorderItem;

            if(_sequence != null)
                _sequence.onSelectionChanged += OnSequenceSelectionChanged;
        }

        private void OnReorderItem(int from, int to)
        {
            UIPuzzleEditor.ExecuteCommand(new Commands.WireReorderCommand(_port.wires, from, to));
        }

        private void OnSequenceSelectionChanged(int selection)
        {
            if (selection == -1)
                return;

            UpdateLabel();
            UpdateWires();
        }

        protected override void OnTargetChanged()
        {
            _port = target.GetValue<Port>();

            _wires.DetachAndDestroyChildren();

            foreach (var wire in wires)
                Instantiate(_wirePrefab, _wires).GetComponent<UIWireEditor>().wire = wire;

            if (_sequence != null)
                _sequence.tile = _port.tile;

            UpdateLabel();
            UpdateWires();

            _portIcon.sprite = DatabaseManager.GetPortIcon(_port);

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

        private UIWireEditor GetWireEditor(int index) =>
            _wires.GetChild(index).GetComponent<UIWireEditor>();
    }
}
