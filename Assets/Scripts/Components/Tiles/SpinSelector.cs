using NoZ;
using UnityEngine;

namespace Puzzled
{
    class SpinSelector : TileComponent
    {
        [Header("Visuals")]
        [SerializeField] private GameObject[] visualValues;

        private int _target = 0;
        private int _value = 0;

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        public Port valueOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(IncrementSignal))]
        public Port incrementPort { get; set; }

        [Editable]
        public int target {
            get => _target;
            set {
                _target = value;
                OnUpdateValue();
            }
        }

        [Editable]
        public int value {
            get => _value;
            set {
                if (visualValues == null || visualValues.Length == 0)
                    _value = 0;
                else
                    _value = 1 + ((value - 1) % visualValues.Length);

                OnUpdateValue();
            }
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => OnUpdateValue();

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            value++;
        }

        [ActorEventHandler]
        private void OnIncrement(IncrementSignal evt) => value++;

        private void OnUpdateValue()
        {
            if (tile == null)
                return;

            valueOutPort.SendValue(value);
            powerOutPort.SetPowered(value == _target);

            for (int i = 0; i < visualValues.Length; ++i)
                visualValues[i].SetActive(value - 1 == i);
        }
    }
}
