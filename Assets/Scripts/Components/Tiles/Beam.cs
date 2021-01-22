using UnityEngine;
using NoZ;
using System.Collections.Generic;

namespace Puzzled
{
    public class Beam : TileComponent
    {
        private class SharedData
        {
            public List<Beam> beams = new List<Beam>();
        }

        [SerializeField] private LineRenderer _line = null;

        private Cell _direction;
        private int _length = 1;

        public Cell direction {
            get => _direction;
            set {
                _direction = value.normalized;
                UpdateBeam();
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
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            var shared = GetSharedData<SharedData>();
            if (null != shared)
                shared.beams.Remove(this);
        }

        private void RayCast (Cell dir)
        {
            var hit = puzzle.RayCast(tile.cell, dir, _length);
            var target = tile.cell + dir * _length;
            if (null != hit)
                target = hit.cell;

            var length = tile.cell.DistanceTo(target);

            // TODO: see if we hit a BeamTerminal, if so connect to it and send the beamchangedevent 

            _line.gameObject.SetActive(true);
            _line.positionCount = 1 + length;
            _line.SetPosition(0, tile.transform.position + Vector3.up * _line.transform.localPosition.y);

            var source = _line.GetPosition(0);
            var dirv = dir.ToVector3();
            for (int i = 0; i < length - 1; i++)
                _line.SetPosition(i + 1, source + dirv * (i + 1));

            _line.SetPosition(_line.positionCount - 1, puzzle.grid.CellToWorld(target) + Vector3.up * _line.transform.localPosition.y);
        }

        private void UpdateBeam ()
        {
            RayCast(_direction);
        }

        public static void Refresh(Puzzle puzzle) => Refresh(puzzle, Cell.invalid);

        public static void Refresh (Puzzle puzzle, Cell cell)
        {
            var shared = puzzle.GetSharedComponentData<SharedData>(typeof(Beam));
            if (null == shared)
                return;

            foreach(var beam in shared.beams)
            {
                beam.UpdateBeam();
            }
        }
    }
}

