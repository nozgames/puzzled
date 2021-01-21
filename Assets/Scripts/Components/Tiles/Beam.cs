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
                _value = (value % _dirs.Length);                
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

        private Port beamInPort { get; set; }
        private Port beamOutPort { get; set; }

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

            //beamInPort = new Port(tile);
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateBeam();
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

        private void RayCast (Cell dir)
        {
            var hit = puzzle.RayCast(tile.cell, dir, 10);
            var target = tile.cell + dir * 10;
            if (null != hit)
                target = hit.cell;

            var length = tile.cell.DistanceTo(target);

            _line.gameObject.SetActive(true);
            _line.positionCount = 1 + length;

            var source = _line.GetPosition(0);
            for (int i = 0; i < length - 1; i++)
                _line.SetPosition(i + 1, source + dir.ToVector3() * (i + 1));

            _line.SetPosition(_line.positionCount - 1, puzzle.grid.CellToWorld(target) + Vector3.up * _line.transform.localPosition.y);
        }

        private Cell[] _dirs = { Cell.up, Cell.right, Cell.down, Cell.left };

        private void UpdateBeam ()
        {
            RayCast(_dirs[_value]);

#if false

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
#endif
        }        
    }
}

