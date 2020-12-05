using UnityEngine;

namespace Puzzled
{
    public class UIOptionWires : UIOptionEditor
    {
        [SerializeField] private Transform _wires = null;
        [SerializeField] private GameObject _wirePrefab = null;
        [SerializeField] private bool _input = true;

        protected override void OnTargetChanged(object target)
        {
            var tile = (Tile)target;

            _wires.DetachAndDestroyChildren();

            var wires = _input ? tile.inputs : tile.outputs;
            foreach (var wire in wires)
            {
                Instantiate(_wirePrefab, _wires).GetComponent<UIOptionWire>().target = wire;
            }
        }
    }
}
