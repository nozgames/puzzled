using NoZ;
using UnityEngine;

namespace Puzzled
{
    class ToggleState : TileComponent
    {
        private bool _hasPower = false;

        protected bool hasPower
        {
            get => _hasPower;
            set
            {
                bool hadPower = _hasPower;
                _hasPower = value;

                if (hadPower != _hasPower)
                    OnPowerChanged();
            }
        }

        public bool isOn { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject visualOn;
        [SerializeField] private GameObject visualOff;

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        private Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            if (powerInPort.wireCount == 0)
                hasPower = true;

            OnPowerChanged();
        }

        [ActorEventHandler]
        private void OnWirePowerChanged(WirePowerChangedEvent evt)
        {
            hasPower = powerInPort.hasPower;
        }

        private void OnPowerChanged()
        {
            UpdateVisuals();
            powerOutPort.SetPowered(hasPower);
        }

        private void UpdateVisuals()
        {
            visualOn.SetActive(hasPower);
            visualOff.SetActive(!hasPower);
        }
    }
}
