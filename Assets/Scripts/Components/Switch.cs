using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Switch : TileComponent
    {
        private bool _on = false;

        [Editable]
        public bool isOn {
            get => _on;
            set {
                if (_on == value)
                    return;

                _on = value;

                UpdateVisuals();
                tile.SetOutputsActive(_on);
            }
        }

        [Header("Visuals")]
        [SerializeField] private GameObject visualOn;
        [SerializeField] private GameObject visualOff;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateVisuals();
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;

            // Toggle the state
            isOn = !isOn;
        }

        private void UpdateVisuals()
        {
            visualOn.SetActive(isOn);
            visualOff.SetActive(!isOn);
        }
    }
}
