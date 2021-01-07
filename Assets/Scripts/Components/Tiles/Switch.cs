using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Switch : TileComponent
    {
        [Header("General")]
        [SerializeField] private bool _usable = true;

        private bool _default = false;
        private bool _on = false;
        private Animator _animator = null;

        /// <summary>
        /// Input power port that is used to disable the switch if power is off
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Power)]
        private Port powerInPort { get; set; }

        // TODO: on/off/reset ports?

        /// <summary>
        /// Output power port that is used to power targets when the switch is on
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        private Port powerOutPort { get; set; }

        /// <summary>
        /// Inport signal port to toggle the switch state
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(ToggleSignal))]
        private Port togglePort { get; set; }

        /// <summary>
        /// Signal port to reset the switch to its default state
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(OnSignal))]
        private Port onPort { get; set; }

        /// <summary>
        /// Signal port to reset the switch to its default state
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(OffSignal))]
        private Port offPort { get; set; }

        /// <summary>
        /// Signal port to reset the switch to its default state
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(ResetSignal))]
        private Port resetPort { get; set; }

        [Editable]
        public bool isOn {
            get => _on;
            set {
                if (_on == value)
                    return;

                _on = value;

                if (value)
                    _animator.SetTrigger(isLoading ? "On" : "OffToOn");
                else
                    _animator.SetTrigger(isLoading ? "Off" : "OnToOff");

                UpdateState();
            }
        }

        protected override void OnAwake ()
        {
            base.OnAwake();

            _animator = GetComponentInChildren<Animator>();

            if (_usable)
                RegisterHandler<UseEvent>();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _default = isOn;
            _animator.SetTrigger(isOn ? "On" : "Off");

            UpdateState();
        }

        [ActorEventHandler(autoRegister = false)]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            isOn = !isOn;
        }

        [ActorEventHandler]
        private void OnToggle(ToggleSignal evt) => isOn = !isOn;

        [ActorEventHandler]
        private void OnOnSignal(OnSignal evt) => isOn = true;

        [ActorEventHandler]
        private void OnOffSignal (OffSignal evt) => isOn = false;

        [ActorEventHandler]
        private void OnResetSignal (ResetSignal evt) => isOn = _default;

        private void UpdateState ()
        {
            if (isLoading)
                return;

            powerOutPort.SetPowered(isOn);
        }
    }
}
