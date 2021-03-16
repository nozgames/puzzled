using NoZ;
using UnityEngine;

namespace Puzzled
{
    class RotateSpinner : UsableSpinner
    {
        [Editable]
        public int numRotations { get; set; } = 8;

        [Editable (serialized = false, hidden = true)]
        public int rotation
        {
            get => value;
            set
            {
                this.value = value;
                ForceRotationIndex(value);
            }

        }
        
        protected override int stepCount => numRotations;
        protected override int maxValues => stepCount;

        protected override Vector3 initialEulerAngles => new Vector3(0, 0, 0);
        protected override Vector3 rotationAxis => new Vector3(0, 1, 0);
    }
}
