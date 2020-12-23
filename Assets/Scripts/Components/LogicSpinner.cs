using NoZ;
using UnityEngine;

namespace Puzzled
{
    class LogicSpinner : TileComponent
    {
        [Editable]
        public int valueCount { get; private set; }

        private int _value = 0;

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            IncrementValue();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateState();

        private void UpdateState()
        {
            tile.SetOutputValue(_value);
            tile.SetOutputsActive(true);
        }

        private void IncrementValue()
        {
            ++_value;
            if (_value >= valueCount)
                _value = 0;

            UpdateState();
        }
    }
}
