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
                _value = (visualValues != null && visualValues.Length > 0) ? (value % visualValues.Length) : 0;
                OnUpdateValue();
            }
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            OnUpdateValue();
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
        }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            IncrementValue();
        }

        private void IncrementValue()
        {
            value++;
            tile.SetOutputValue(value);
        }

        private void OnUpdateValue()
        {
            if (tile == null)
                return;

            if (value == _target)
                tile.SetOutputsActive(true);
            else
                tile.SetOutputsActive(false);

            for (int i = 0; i < visualValues.Length; ++i)
                visualValues[i].SetActive(value == i);
        }
    }
}
