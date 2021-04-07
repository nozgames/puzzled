using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Rotatable : TileComponent
    {
        private int _rotationIndex = 0;

        [SerializeField]
        public int numRotations = 4;

        [Editable(hidden = true)]
        public int rotation
        {
            get => _rotationIndex;
            set
            {
                this._rotationIndex = value % numRotations;
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
            float rotationStep = 360 / numRotations;
            transform.localRotation = Quaternion.Euler(0, _rotationIndex * rotationStep, 0);
        }
    }
}
