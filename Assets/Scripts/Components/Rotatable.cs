using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Rotatable : TileComponent
    {
        private bool _rotated = false;

        [Editable(hidden = true)]
        public bool rotated {
            get => _rotated;
            set {
                _rotated = value;
                UpdateRotation();
            }
        }

        [ActorEventHandler]
        private void OnStart (StartEvent evt)
        {
            UpdateRotation();
        }

        private void UpdateRotation()
        {
            transform.localRotation = Quaternion.Euler(0, _rotated ? 90 : 0, 0);
        }
    }
}
