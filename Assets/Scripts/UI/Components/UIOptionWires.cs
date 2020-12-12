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
        [SerializeField] private TMPro.TextMeshProUGUI _noinputs  = null;

        public bool isInput => _input;
        public bool isReorderable => _reorderable;

        private void OnEnable()
        {
        }

        protected override void OnTargetChanged(object target)
        {
            var tile = (Tile)target;

            label = _input ? "Inputs" : "Outputs";

            _noinputs.text = $"No {label}";

            _wires.DetachAndDestroyChildren();

            var wires = _input ? tile.inputs : tile.outputs;
            foreach (var wire in wires)
            {
                Instantiate(_wirePrefab, _wires).GetComponent<UIOptionWire>().target = wire;
            }

            UpdateWires();

            OnSelectionChanged(null);
        }

        internal void OnSelectionChanged(UIOptionWire wireOption)
        {
        }

        public void UpdateWires()
        {
            _noinputs.transform.parent.gameObject.SetActive(_wires.childCount <= 0);
            _wires.gameObject.SetActive(_wires.childCount > 0);

            for (int i = 0; i < _wires.childCount; i++)
                _wires.GetChild(i).GetComponent<UIOptionWire>().UpdateIndex();
        }

        public void Select(int index)
        {
            if (_wires.childCount == 0)
                return;

            _wires.GetChild(Mathf.Clamp(index, 0, _wires.childCount - 1)).GetComponent<UIOptionWire>().Select();
        }
    }
}
