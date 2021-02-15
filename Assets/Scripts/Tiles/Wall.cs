using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Wall : TileComponent
    {
        [SerializeField] private GameObject _visuals = null;
        [SerializeField] private GameObject _capLeft = null;
        [SerializeField] private GameObject _capRight = null;

        [SerializeField] private int _capPriority = 0;
        [SerializeField] private bool _smartCorners = true;

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateVisuals(tile.cell, isEditing);
        }

        [ActorEventHandler]
        private void CellChangedEvent(CellChangedEvent evt)
        {
            if (!isEditing)
                return;

            UpdateVisuals(evt.old, true);
            UpdateVisuals(tile.cell, true);
        }

        [ActorEventHandler]
        private void OnQueryMoveEvent (QueryMoveEvent evt)
        {
            var edge = tile.cell.edge;
            if (evt.offset.x > 0 && edge == Cell.Edge.East)
                evt.result = false;
            else if (evt.offset.x < 0 && edge == Cell.Edge.East)
                evt.result = false;
            else if (evt.offset.y < 0 && edge == Cell.Edge.North)
                evt.result = false;
            else if (evt.offset.y > 0 && edge == Cell.Edge.North)
                evt.result = false;
        }

        private Wall GetParallel (Cell cell, int cap)
        {
            switch (cell.edge)
            {
                case Cell.Edge.North:
                    return puzzle.grid.CellToComponent<Wall>(new Cell(cell + (cap == 0 ? Cell.west : Cell.east), Cell.Edge.North), TileLayer.Wall);

                case Cell.Edge.East:
                    return puzzle.grid.CellToComponent<Wall>(new Cell(cell + (cap == 0 ? Cell.north : Cell.south), Cell.Edge.East), TileLayer.Wall);

                default:
                    return null;
            }
        }

        private Wall GetPerpendicular (Cell cell, int cap, int dir)
        {
            switch (cell.edge)
            {
                case Cell.Edge.North:
                    return dir == 0 ?
                        puzzle.grid.CellToComponent<Wall>(new Cell(cell + (cap == 0 ? Cell.west : Cell.zero) + Cell.north, Cell.Edge.East), TileLayer.Wall) :
                        puzzle.grid.CellToComponent<Wall>(new Cell(cell + (cap == 0 ? Cell.west : Cell.zero), Cell.Edge.East), TileLayer.Wall);

                case Cell.Edge.East:
                    return dir == 0 ?
                        puzzle.grid.CellToComponent<Wall>(new Cell(cell + (cap == 0 ? Cell.south : Cell.zero), Cell.Edge.North), TileLayer.Wall) :
                        puzzle.grid.CellToComponent<Wall>(new Cell(cell + (cap == 0 ? Cell.south : Cell.zero) + Cell.east, Cell.Edge.North), TileLayer.Wall);

                default:
                    return null;
            }
        }

        private bool IsHigherPriority (Wall compare)
        {
            if (compare == null)
                return true;

            if (_capPriority > compare._capPriority)
                return true;

            if (_capPriority < compare._capPriority)
                return false;

            // Priority is the same so use the edge to choose one
            var cell = tile.cell.NormalizeEdge();
            var compareCell = compare.tile.cell.NormalizeEdge();

            if (cell.edge > compareCell.edge)
                return true;

            if (cell.edge < compareCell.edge)
                return false;

            // Edge is the same so use x/y to choose one
            return cell.x > compareCell.x || cell.y > compareCell.y;
        }

        private bool UpdateCap (Cell cell, int cap, bool updateNeighbors)
        {
            var wallParallel = GetParallel(cell, cap);
            var wallPerpendicular0 = GetPerpendicular(cell, cap, 0);
            var wallPerpendicular1 = GetPerpendicular(cell, cap, 1);

            if(updateNeighbors)
            {
                if (wallParallel != null)
                    wallParallel.UpdateVisuals(wallParallel.tile.cell, false);
                if (wallPerpendicular0 != null)
                    wallPerpendicular0.UpdateVisuals(wallPerpendicular0.tile.cell, false);
                if (wallPerpendicular1 != null)
                    wallPerpendicular1.UpdateVisuals(wallPerpendicular1.tile.cell, false);
            }

            if (cell != tile.cell.NormalizeEdge())
                return false;

            if (!IsHigherPriority(wallParallel))
                return false;

            if (!IsHigherPriority(wallPerpendicular0))
                return false;

            if (!IsHigherPriority(wallPerpendicular1))
                return false;

            var perpendicularWalls = wallPerpendicular0 != null || wallPerpendicular1 != null;
            var parallelWall = wallParallel != null;

            // Always cap the end
            // TODO: optional?
            if (!parallelWall && !perpendicularWalls)
                return true;

            // If using smart corners then dot cap perpendicular only transitions
            if (_smartCorners && !perpendicularWalls)
                return false;

            return true;
        }

        private void UpdateVisuals(Cell cell, bool updateNeighbors = false)
        {
            if (cell == Cell.invalid)
                return;

            cell = cell.NormalizeEdge();
            switch (cell.edge)
            {
                case Cell.Edge.North:
                    _visuals.transform.localPosition = new Vector3(0, 0, 0.5f);
                    break;

                case Cell.Edge.East:
                    _visuals.transform.localEulerAngles = new Vector3(0, 90, 0);
                    _visuals.transform.localPosition = new Vector3(0.5f, 0, 0);
                    break;

                case Cell.Edge.None:
                    return;

                default:
                    throw new System.NotSupportedException();
            }

            var capLeft = UpdateCap(cell, 0, updateNeighbors);
            var capRight = UpdateCap(cell, 1, updateNeighbors);

            if (_capLeft != null)
                _capLeft.gameObject.SetActive(capLeft);

            if (_capRight != null)
                _capRight.gameObject.SetActive(capRight);
        }

#if false
        [SerializeField] private GameObject _visualWest = null;
        [SerializeField] private GameObject _visualNorth = null;

        private Wall GetWall(Cell cell) => puzzle.grid.CellToComponent<Wall>(cell, TileLayer.Static);

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateVisuals();

            UpdateNeighbors(tile.cell);
        }

        private void UpdateNeighbors (Cell cell)
        {
            var east = GetWall(cell + Cell.right);
            if (east != null)
                east.UpdateVisuals();

            var south = GetWall(cell + Cell.down);
            if (south != null)
                south.UpdateVisuals();
        }

        [ActorEventHandler]
        private void CellChangedEvent (CellChangedEvent evt)
        {
            UpdateNeighbors(evt.old);
            UpdateNeighbors(tile.cell);
            UpdateVisuals();
        }

        private void UpdateVisuals()
        { 
            _visualWest.SetActive(GetWall(tile.cell + Cell.left) != null);
            _visualNorth.SetActive(GetWall(tile.cell + Cell.up) != null);
        }
#endif
    }
}
