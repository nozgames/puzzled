using NoZ;

namespace Puzzled
{
    public class UsableTileComponent : TileComponent
    {
        private bool _isUsable = false;

        protected bool isUsable
        {
            get => _isUsable;
            set 
            {
                bool hadPower = _isUsable;
                _isUsable = value;

                if (hadPower != _isUsable)
                    OnUsableChanged();
            }
        }

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateUsable();

        [ActorEventHandler]
        private void OnWirePowerChanged(WirePowerChangedEvent evt) => UpdateUsable();

        private void UpdateUsable()
        {
            var oldUsable = _isUsable;
            _isUsable = powerInPort.wireCount == 0 || powerInPort.hasPower;
            if (_isUsable != oldUsable)
                OnUsableChanged();
        }

        protected virtual void OnUsableChanged()
        {
        }
    }
}
