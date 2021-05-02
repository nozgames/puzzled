using System.Collections.Generic;
using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Usable))]
    class Switch : TileComponent
    {
        private class SharedData
        {
            public List<Switch> global = new List<Switch>();
        }

        [Header("General")]
        [SerializeField] private bool _usable = true;
        [SerializeField] private Animator _animator = null;
        [SerializeField] private AudioClip _onSound = null;
        [SerializeField] private AudioClip _offSound = null;

        private bool _default = false;
        private bool _on = false;
        private int _animationOn;
        private int _animationOff;
        private string _globalId;

        // TODO: on/off/reset ports?

        /// <summary>
        /// Output power port that is used to power targets when the switch is on
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        private Port powerOutPort { get; set; }

        /// <summary>
        /// Output value (0 or 1)
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        private Port valueOutPort { get; set; }

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

                // Update the global setting
#if false
                if (isGlobal && !isLoading)
                {
                    PlayerPrefs.SetInt(globalKey, _on ? 1 : 0);
                    var shared = GetSharedData<SharedData>();
                    if (null != shared)
                        foreach (var globalSwitch in shared.global)
                            if (globalSwitch.isOn != _on && 0 == string.Compare(globalSwitch.globalId, globalId, true))
                                globalSwitch.isOn = _on;
                }
#endif

                UpdateAnimation();

                PlaySound(value ? _onSound : _offSound);

                UpdateState();
            }
        }

        /// <summary>
        /// Returns true if the switch is a global switch
        /// </summary>
        private bool isGlobal => !string.IsNullOrWhiteSpace(globalId);

#if false
        /// <summary>
        /// Returns the global key for the switch
        /// </summary>
        private string globalKey => $"{puzzle.worldName}.{puzzle.name}.{globalId}";
#endif

        [Editable(placeholder = "None")]
        private string globalId {
            get => _globalId;
            set {
                // Dont allow a key of none
                if (value != null && 0 == string.Compare(value, "none", true))
                    _globalId = null;
                else
                    _globalId = value;
            }
        }

        private void Awake()
        {
            if (null != _animator)
            {
                _animationOn = Animator.StringToHash("On");
                _animationOff = Animator.StringToHash("Off");
            }
        }

        [ActorEventHandler()]
        private void OnAwake (AwakeEvent evt)
        {
            if (_usable)
                RegisterHandler<UseEvent>();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            if (!isEditing && isGlobal)
            {
                var shared = GetSharedData<SharedData>();
                if (null == shared)
                {
                    shared = new SharedData();
                    SetSharedData(shared);
                }

                shared.global.Add(this);

#if false
                var saved = PlayerPrefs.GetInt(globalKey, -1);
                if (saved != -1)
                    _on = saved == 1;
#endif
            }

            _default = _on;

            UpdateAnimation();
            UpdateState();
        }

        [ActorEventHandler]
        private void OnDestroyEvent (DestroyEvent evt)
        {
            if(!isEditing)
            {
                var shared = GetSharedData<SharedData>();
                if (null != shared)
                    shared.global.Remove(this);
            }
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
            valueOutPort.SendValue(isOn ? 1 : 0, true);
        }

        private void UpdateAnimation ()
        {
            if (_animator != null)
            {
                _animator.enabled = true;
                _animator.Play(isOn ? _animationOn : _animationOff, 0, (isEditing || isLoading || isStarting) ? 1.0f : 0.0f);
            }
        }
    }
}
