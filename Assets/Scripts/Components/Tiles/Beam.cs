using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Beam : TileComponent
    {
        [SerializeField] private LineRenderer _line = null;
        [SerializeField] private AudioClip _useSound = null;
        [SerializeField] private GameObject _visualsOn = null;
        [SerializeField] private GameObject _visualsOff = null;

        private int _value = 0;
        private bool _terminal = false;

        [Editable]        
        private int value {
            get => _value;
            set {
                if (powerOutPort.wireCount == 0)
                    _value = 0;
                else
                    _value = (value % powerOutPort.wireCount);                
                UpdateBeam();
            }
        }

        [Editable]
        private bool isTerminal {
            get => _terminal;
            set {
                _terminal = value;
                UpdateBeam();
            }
        }

        [Editable]
        [Port(PortFlow.Input, PortType.Power)]
        private Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power)]
        private Port powerOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Number)]
        private Port valueInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Number)]
        private Port valueOutPort { get; set; }

        [ActorEventHandler]
        private void OnAwakeEvent(AwakeEvent evt)
        {
            _line.positionCount = 2;
            _line.gameObject.SetActive(false);
        }

        [ActorEventHandler]
        private void OnCellChangeEvent (CellChangedEvent evt)
        {
            _line.SetPosition(0, tile.transform.position + Vector3.up * _line.transform.localPosition.y);
        }

        [ActorEventHandler]
        private void OnWirePowerChangedEvent (WirePowerChangedEvent evt)
        {
            UpdateBeam();
        }

        [ActorEventHandler]
        private void OnTickEvent (TickEvent evt)
        {
            //UpdateBeam();
        }

        [ActorEventHandler]
        private void OnUseEvent (UseEvent evt)
        {
            evt.IsHandled = true;
            value++;
            PlaySound(_useSound);
        }

        [ActorEventHandler]
        private void OnValueEvent (ValueEvent evt)
        {
            value = evt.value - 1;
        }

        private void UpdateBeam ()
        {
            _visualsOn.SetActive(powerInPort.hasPower);
            _visualsOff.SetActive(!powerInPort.hasPower);

            if (_terminal || !powerInPort.hasPower || _value < 0 || _value >= powerOutPort.wireCount)
            {
                powerOutPort.SetPowered(_terminal && powerInPort.hasPower);
                _line.gameObject.SetActive(false);
                return;
            }

            var source = _line.GetPosition(0);
            var target = powerOutPort.GetWire(_value).to.tile.transform.position + Vector3.up * _line.transform.localPosition.y;
            var length = (int)Mathf.Ceil((target - source).magnitude - 0.5f);
            var dir = (target - source).normalized;

            _line.positionCount = 1 + length;

            for (int i = 0; i < length - 1; i++)
                _line.SetPosition(i + 1, source + dir * (i + 1));

            _line.SetPosition(_line.positionCount - 1, target);

            powerOutPort.SetPowered(false);
            powerOutPort.SetPowered(_value, true);
            valueOutPort.SendValue(_value + 1, true);

            _line.gameObject.SetActive(true);
        }
    }
}
