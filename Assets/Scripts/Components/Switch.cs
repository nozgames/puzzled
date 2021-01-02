using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Switch : TileComponent
    {
        [Header("General")]
        [SerializeField] private bool _usable = true;

        [Header("Visuals")]
        [SerializeField] private GameObject visualOn = null;
        [SerializeField] private GameObject visualOff = null;

        private bool _on = false;

        /// <summary>
        /// Inport signal port to toggle the switch state
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(ToggleEvent))]
        public Port togglePort { get; set; }

        /// <summary>
        /// Input power port that is used to disable the switch if power is off
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Power)]
        public Port powerInPort { get; set; }

        // TODO: on/off/reset ports?

        /// <summary>
        /// Output power port that is used to power targets when the switch is on
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [Editable]
        public bool isOn {
            get => _on;
            set {
                if (_on == value)
                    return;

                _on = value;

                UpdateState();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if(_usable)
                RegisterHandler<UseEvent>();
        }

        protected override void OnDisable()
        {
            if (_usable)
                UnregisterHandler<UseEvent>();

            base.OnDisable();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateState();

        [ActorEventHandler(autoRegister = false)]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            isOn = !isOn;
        }

        [ActorEventHandler]
        private void OnToggle(ToggleEvent evt) => isOn = !isOn;

        private void UpdateState ()
        {
            if(visualOn != null)
                visualOn.SetActive(isOn);

            if(visualOff != null)
                visualOff.SetActive(!isOn);
        }
    }
}
