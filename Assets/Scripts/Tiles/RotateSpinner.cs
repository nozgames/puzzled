using NoZ;
using UnityEngine;

namespace Puzzled
{
    class RotateSpinner : UsableSpinner
    {
        const int kNumRotations = 8;

        protected override int stepCount => kNumRotations;
        protected override int maxValues => kNumRotations;

        protected override Vector3 initialEulerAngles => new Vector3(0, 0, 0);
        protected override Vector3 rotationAxis => new Vector3(0, 1, 0);
    }
}
