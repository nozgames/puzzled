using UnityEngine;
using NoZ;
using System.Collections.Generic;
using UnityEngine.VFX;

namespace Puzzled
{
    public enum BeamDirection
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    public class Beam : TileComponent
    {
        private class SharedData
        {
            public List<Beam> beams = new List<Beam>();
        }

        private static readonly Cell[] _dirs = {
            Cell.north,
            Cell.northEast,
            Cell.east,
            Cell.southEast,
            Cell.south,
            Cell.southWest,
            Cell.west,
            Cell.northWest
        };

        [SerializeField] private LineRenderer _line = null;
        [SerializeField] private VisualEffect _impactFX = null;

        private BeamDirection _direction;
        private int _length = 1;
        private BeamTerminal _terminal = null;
        private bool _powered = false;

        public BeamDirection direction {
            get => _direction;
            set {
                if (_direction == value)
                    return;
                _direction = value;
                UpdateBeam();
            }
        }

        public static Quaternion GetRotation (BeamDirection dir) =>
            Quaternion.LookRotation(_dirs[(int)dir].ToVector3().normalized, Vector3.up);

        public bool isPowered {
            get => _powered;
            set {
                if (_powered == value)
                    return;

                _powered = value;
                if(_terminal != null)
                {
                    _terminal.Disconnect(this);
                    _terminal.Connect(this);
                }
            }
        }

        public int length {
            get => _length;
            set {
                _length = Mathf.Clamp(value, 1, 100);
                UpdateBeam();
            }
        }

        [ActorEventHandler]
        private void OnAwakeEvent(AwakeEvent evt)
        {
            _line.positionCount = 2;
            _line.gameObject.SetActive(false);
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateBeam();

        [ActorEventHandler]
        private void OnCellChangeEvent(CellChangedEvent evt) => UpdateBeam();

        protected override void OnEnable()
        {
            base.OnEnable();

            var shared = GetSharedData<SharedData>();
            if(null == shared)
            {
                shared = new SharedData();
                SetSharedData(shared);
            }

            shared.beams.Add(this);

            UpdateBeam();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            var shared = GetSharedData<SharedData>();
            if (null != shared)
                shared.beams.Remove(this);

            Disconnect();
        }

        public void Disconnect()
        {
            if (_terminal == null)
                return;

            var terminal = _terminal;
            _terminal = null;
            terminal.Disconnect(this);
        }

        private void UpdateBeam ()
        {
            if (!isActiveAndEnabled)
                return;

            var dir = _dirs[(int)_direction];
            var raycast = puzzle.RayCast(tile.cell, dir, _length);
            var target = tile.cell + dir * _length;
            if (null != raycast.hit)
                target = raycast.hit.cell;

            var length = tile.cell.DistanceTo(target);

            var terminal = raycast.hit != null ? raycast.hit.GetComponent<BeamTerminal>() : null;
            if (terminal != _terminal)
                Disconnect();

            if (null != terminal)
            {
                _terminal = terminal;
                terminal.Connect(this);
            }

            _line.gameObject.SetActive(true);
            _line.positionCount = 1 + length;
            _line.SetPosition(0, tile.transform.position + Vector3.up * _line.transform.localPosition.y);

            var source = _line.GetPosition(0);
            var dirv = dir.ToVector3();
            for (int i = 0; i < length - 1; i++)
                _line.SetPosition(i + 1, source + dirv * (i + 1));

            _line.SetPosition(_line.positionCount - 1, puzzle.grid.CellToWorldBounds(target).center + Vector3.up * _line.transform.localPosition.y - dirv * raycast.offset);

            if (_impactFX != null)
            {
                _impactFX.transform.rotation = Quaternion.LookRotation(_line.GetPosition(0) - _line.GetPosition(1), Vector3.up);
                _impactFX.transform.position = _line.GetPosition(_line.positionCount - 1);
                _impactFX.gameObject.SetActive(raycast.hit != null);
            }
        }

        public static void Refresh(Puzzle puzzle) => Refresh(puzzle, Cell.invalid);

        public static void Refresh (Puzzle puzzle, Cell cell)
        {
            var shared = puzzle.GetSharedComponentData<SharedData>(typeof(Beam));
            if (null == shared)
                return;

            for(int i=0; i<shared.beams.Count; i++)
                shared.beams[i].UpdateBeam();
        }
    }
}

