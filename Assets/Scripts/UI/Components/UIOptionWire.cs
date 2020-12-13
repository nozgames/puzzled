using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    class UIOptionWire : UIOptionEditor
    {
        [Header("General")]
        [SerializeField] private Toggle _toggle = null;
        [SerializeField] private TMPro.TextMeshProUGUI _tileName = null;
        [SerializeField] private GameObject _buttons = null;
        [SerializeField] private Button _moveUpButtom = null;
        [SerializeField] private Button _moveDownButton = null;

        [Header("Parameters")]
        [SerializeField] private Toggle _param1Toggle = null;

        private UIOptionWires wiresEditor = null;

        private void Awake()
        {
            _toggle.group = GetComponentInParent<ToggleGroup>();
            wiresEditor = GetComponentInParent<UIOptionWires>();            
        }

        protected override void OnTargetChanged(object target)
        {
            var wire = (Wire)target;
            var tile = wiresEditor.isInput ? wire.from.tile : wire.to.tile;

            _tileName.text = tile.info.displayName;

            if (_param1Toggle != null)
            {
                _param1Toggle.isOn = (wiresEditor.isInput ? wire.to.GetOption(0) : wire.from.GetOption(0)) == 1;
                _param1Toggle.onValueChanged.AddListener((value) => {
                    if (wiresEditor.isInput)
                        wire.to.SetOption(0, value ? 1 : 0);
                    else
                        wire.from.SetOption(0, value ? 1 : 0);
                });
            }

            UpdateIndex();
        }

        public void OnSelectionChanged (bool selected)
        {
            if(selected)
                wiresEditor.OnSelectionChanged(this);

            var wire = (Wire)target;
            wire.selected = selected;

            _buttons.SetActive(selected);
            if(selected)
                UpdateButtons();
        }

        public void UpdateIndex()
        {
            label = (transform.GetSiblingIndex() + 1).ToString();
            UpdateButtons();
        }

        public void OnMoveUpButton()
        {
            var wire = ((Wire)target);
            var index = wiresEditor.isInput ? wire.to.tile.GetInputIndex(wire) : wire.from.tile.GetOutputIndex(wire);
            if (index == 0)
                return;

            if (wiresEditor.isInput)
                wire.to.tile.SetInputIndex(wire, index - 1);
            else
                wire.from.tile.SetOutputIndex(wire, index - 1);

            transform.SetSiblingIndex(index - 1);

            wiresEditor.UpdateWires();
            UpdateButtons();
        }

        public void OnMoveDownButton()
        {
            var wire = ((Wire)target);
            if (wiresEditor.isInput)
            {
                var index = wire.to.tile.GetInputIndex(wire);
                if (index >= wire.to.tile.inputCount - 1)
                    return;

                wire.to.tile.SetInputIndex(wire, index + 1);
            } else
            {
                var index = wire.from.tile.GetOutputIndex(wire);
                if (index >= wire.from.tile.outputCount - 1)
                    return;

                wire.from.tile.SetOutputIndex(wire, index + 1);
            }

            transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);

            wiresEditor.UpdateWires();
            UpdateButtons();
        }

        public void OnDeleteButton()
        {
            var index = transform.GetSiblingIndex();
            _toggle.isOn = false;

            // Destroy the wire
            Destroy(((Wire)target).gameObject);

            // Destroy the wire entry
            gameObject.transform.SetParent(null);
            Destroy(gameObject);

            wiresEditor.UpdateWires();
            wiresEditor.Select(index);
        }

        private void UpdateButtons()
        {
            _moveUpButtom.interactable = transform.GetSiblingIndex() > 0;
            _moveDownButton.interactable = transform.GetSiblingIndex() < transform.parent.childCount - 1;
        }

        public void Select()
        {
            _toggle.isOn = true;
        }
    }
}
