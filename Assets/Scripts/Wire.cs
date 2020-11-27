using UnityEngine;

namespace Puzzled
{
    public class Wire
    {
        private bool _active = false;

        public LineRenderer line = null;

        public Tile input { get; set; }
        public Tile output { get; set; }
        
        public bool active {
            get => _active;
            set {
                if (_active == value)
                    return;

                _active = value;
                if (_active)
                    output.Send(new ActivateWireEvent(this));
                else
                    output.Send(new DeactivateWireEvent(this));
            }
        }
    }
}
