using NoZ;
using UnityEngine;

namespace Puzzled
{
    abstract class Spinner : TileComponent
    {
        private int _target = 0;
        private int _value = 0;

        abstract protected int maxValues { get; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power, legacy = true)]
        public Port powerOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        public Port valueOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(IncrementSignal))]
        public Port incrementPort { get; set; }

        [Editable]
        public int target
        {
            get => _target;
            set
            {
                _target = value;
                UpdatePortValues();
            }
        }

        [Editable]
        public int value
        {
            get => _value;
            set
            {
                if (value == _value)
                    return;

                _value = isLoading ? value : ClampedValue(value);
                UpdatePortValues();
            }
        }

        protected int ClampedValue(int value) => (maxValues > 0) ? Mathf.Clamp(value % maxValues, 0, maxValues - 1) : 0;
        protected int WrappedValue(int value) => (maxValues > 0) ? ((value + maxValues) % maxValues) : 0;

        [ActorEventHandler]
        private void OnIncrement(IncrementSignal evt)
        {
            if (maxValues == 0)
                return;

            value = value + 1;
        }

        [ActorEventHandler]
        protected virtual void OnStart(StartEvent evt)
        {
            value = ClampedValue(value); // just in case it didn't happen during loading
        }

        private void UpdatePortValues()
        {
            if (tile == null || isLoading)
                return;

            valueOutPort.SendValue(_value, true);
            powerOutPort.SetPowered(_value == _target);
            OnPortValuesUpdated();
        }

        protected virtual void OnPortValuesUpdated()
        {
        }
    }
}
