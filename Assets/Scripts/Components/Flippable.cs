using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Flippable : TileComponent
    {
        private bool _flipped = false;

        [Editable(hidden = true)]
        public bool flipped {
            get => _flipped;
            set {
                _flipped = value;
                UpdateScale();
            }
        }

        [ActorEventHandler]
        private void OnStart (StartEvent evt)
        {
            UpdateScale();
        }

        private void UpdateScale()
        {
            transform.localScale = new Vector3(1.0f, 1.0f, _flipped ? -1.0f : 1.0f);
        }
    }
}
