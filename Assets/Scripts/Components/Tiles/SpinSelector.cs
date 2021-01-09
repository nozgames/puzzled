using NoZ;
using UnityEngine;

namespace Puzzled
{
    class SpinSelector : TileComponent
    {
        [Header("Visuals")]
        [SerializeField] private GameObject[] visualValues;
        [SerializeField] private Transform _rotator = null;

        private int _target = 1;
        private int _value = 1;

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
                if (value == _value)
                    return;

                var old = _value;

                if (visualValues == null || visualValues.Length == 0)
                    _value = 1;
                else
                    _value = Mathf.Clamp(1 + ((value - 1) % visualValues.Length), 1, visualValues.Length);


                var step = (360.0f / 10.0f);
                if (!isLoading && !isEditing)
                    Tween.Rotate(
                        new Vector3(0, -90, 90 + step * (_value - 2) - 180),
                        new Vector3(0, -90, 90 + step * (_value - 1) - 180))
                        .Duration(0.25f)
                        .Start(_rotator.gameObject);

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

            valueOutPort.SendValue(value, true);
            powerOutPort.SetPowered(value == _target);

            //for (int i = 0; i < visualValues.Length; ++i)
            //   visualValues[i].GetComponent<SpriteRenderer>().color = (value-1) == i ? Color.white : Color.grey;
        }
    }
}
