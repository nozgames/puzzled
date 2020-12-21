using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    class UIOptionWire : UIOptionEditor
    {
        [Header("General")]
        [SerializeField] private TMPro.TextMeshProUGUI _tileName = null;
        [SerializeField] private UIListItem _item = null;

        [Header("Parameters")]
        [SerializeField] private Toggle _param1Toggle = null;

        private UIOptionWires wiresEditor = null;
        private Wire _wire = null;

        public Wire wire => _wire;

        private int wireOption {
            get => wiresEditor.isInput ? _wire.to.GetOption(0) : _wire.from.GetOption(0);
            set {
                if (wiresEditor.isInput)
                    _wire.to.SetOption(0, value);
                else
                    _wire.from.SetOption(0, value);
            }
        }

        private int wireToggleMask => wiresEditor.sequence != null ? (1<<wiresEditor.sequence.selection) : 1;

        private void Awake()
        {
            wiresEditor = GetComponentInParent<UIOptionWires>();                      
        }

        private void OnEnable()
        {
            _item.onSelectionChanged.AddListener(OnSelectionChanged);
            UIPuzzleEditor.onSelectedWireChanged += OnWireSelectionChanged;

            if (_param1Toggle != null && wiresEditor.sequence != null)
            {
                wiresEditor.sequence.onStepRemoved += OnSequenceStepRemoved;
                wiresEditor.sequence.onStepMoved += OnSequenceStepMoved;
            }            
        }

        private void OnDisable()
        {
            if (_param1Toggle != null && wiresEditor.sequence != null)
            {
                wiresEditor.sequence.onStepRemoved -= OnSequenceStepRemoved;
                wiresEditor.sequence.onStepMoved -= OnSequenceStepMoved;
            }

            _item.onSelectionChanged.RemoveListener(OnSelectionChanged);
            UIPuzzleEditor.onSelectedWireChanged -= OnWireSelectionChanged;
        }

        protected override void OnTargetChanged(object target)
        {
            _wire = (Wire)target;
            if (null == _wire)
                return;

            var tile = wiresEditor.isInput ? _wire.from.tile : _wire.to.tile;

            _tileName.text = tile.info.displayName;

            UpdateState();

            UpdateIndex();

            OnWireSelectionChanged(UIPuzzleEditor.selectedWire);
        }

        private void OnSequenceStepMoved(int from, int to)
        {
            var value = (wireOption >> from) & 1;

            // Remove the bit
            var mask = (1 << from) - 1;
            wireOption = (wireOption & mask) | ((wireOption >> 1) & ~mask);

            // Insert the bit
            mask = (1 << to) - 1;
            wireOption = ((wireOption & mask) | ((wireOption << 1) & ~mask)) & (~(1 << to)) | (value << to);
        }

        private void OnSequenceStepRemoved(int step)
        {
            // Remove the bit for the step
            var mask = (1 << step) - 1;
            wireOption = (wireOption & mask) | ((wireOption >> 1) & ~mask);
        }

        public void UpdateState ()
        {
            UpdateIndex();

            if (_param1Toggle != null)
            {
                _param1Toggle.onValueChanged.RemoveAllListeners();
                _param1Toggle.isOn = (wireOption & wireToggleMask) == wireToggleMask;
                _param1Toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        private void OnToggleValueChanged(bool value)
        {
            var mask = wireToggleMask;
            wireOption = (wireOption & (~mask)) | (value ? mask : 0);
        }

        private void OnWireSelectionChanged(Wire selectedWire)
        {
            _item.selected = (selectedWire == _wire);
        }

        public void OnSelectionChanged (bool selected)
        {
            if (null == target)
                return;

            if (selected)
                UIPuzzleEditor.selectedWire = (Wire)target;
        }

        public void UpdateIndex()
        {
            label = (transform.GetSiblingIndex() + 1).ToString();            
        }

        public void Select() => _item.selected = true;
    }
}
