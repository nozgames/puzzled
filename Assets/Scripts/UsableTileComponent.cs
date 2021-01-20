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
        private void OnStart(StartEvent evt)
        {
            if (powerInPort.wireCount == 0)
                isUsable = true;

            OnUsableChanged();
        }

        [ActorEventHandler]
        private void OnWirePowerChanged(WirePowerChangedEvent evt)
        {
            isUsable = powerInPort.hasPower;
        }

        protected virtual void OnUsableChanged()
        {
        }
    }
}
