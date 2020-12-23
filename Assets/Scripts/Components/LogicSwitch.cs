using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicSwitch : TileComponent
    {
        private bool _on = false;

        [Editable]
        public bool isOn
        {
            get => _on;
            set
            {
                if (_on == value)
                    return;

                _on = value;

                if (tile != null)
                    tile.SetOutputsActive(_on);
            }
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            tile.SetOutputsActive(_on);
        }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            ToggleSwitchState();
        }

        private void ToggleSwitchState()
        {
            isOn = !isOn;
        }
    }
}
