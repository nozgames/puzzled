using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Switch : TileComponent
    {
        public bool isOn { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject visualOn;
        [SerializeField] private GameObject visualOff;

        [ActorEventHandler]
        private void OnQueryUse(QueryUseEvent evt)
        {
            evt.result = true;
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            if (isOn)
                TurnOff();
            else
                TurnOn();
        }

        private void TurnOn()
        {
            isOn = true;
            UpdateVisuals();
            tile.SetOutputsActive(true);
        }

        private void TurnOff()
        {
            isOn = false;
            UpdateVisuals();
            tile.SetOutputsActive(false);
        }

        private void UpdateVisuals()
        {
            visualOn.SetActive(isOn);
            visualOff.SetActive(!isOn);
        }
    }
}
