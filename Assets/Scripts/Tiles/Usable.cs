using NoZ;

namespace Puzzled
{
    public class Usable : TileComponent
    {
        private bool _isUsable = false;
        private Tooltip _tooltip = null;

        [ActorEventHandler (priority = int.MaxValue)]
        private void OnUse(UseEvent evt)
        {
            if (!_isUsable)
                evt.IsHandled = true;
        }

        private void Awake()
        {
            _tooltip = GetComponent<Tooltip>();
        }

        public bool isUsable => _isUsable;

        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateUsable();

        [ActorEventHandler]
        private void OnWirePowerChanged(WirePowerChangedEvent evt) => UpdateUsable();

        private void UpdateUsable()
        {
            bool oldUsable = _isUsable;
            _isUsable = powerInPort.wireCount == 0 || powerInPort.hasPower;

            if (_isUsable != oldUsable)
                Send(new UsableChangedEvent(this));

            if (_tooltip != null)
                _tooltip.enabled = _isUsable;
        }
    }
}
