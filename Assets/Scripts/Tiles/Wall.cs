using UnityEngine;
using NoZ;
using System.Collections.Generic;

namespace Puzzled
{
    public class Wall : TileComponent
    {
        [SerializeField] private GameObject _visuals = null;
        [SerializeField] private GameObject _capLeft = null;
        [SerializeField] private GameObject _capRight = null;

        [SerializeField] private int _capPriority = 0;
        [SerializeField] private bool _smartCorners = true;

        [SerializeField] private bool _allowWallMounts = true;
        [SerializeField] private float _thickness = 0.1f;

        public float thickness => _thickness;

        /// <summary>
        /// True if the wall allows tils to be attached to it
        /// </summary>
        public bool allowsWallMounts => _allowWallMounts;

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
            if (evt.offset.x > 0 && edge == CellEdge.East)
                evt.result = false;
            else if (evt.offset.x < 0 && edge == CellEdge.East)
                evt.result = false;
            else if (evt.offset.y < 0 && edge == CellEdge.North)
                evt.result = false;
            else if (evt.offset.y > 0 && edge == CellEdge.North)
                evt.result = false;
        }

        private Wall GetParallel(Cell cell, CellEdgeSide cap) => 
            puzzle.grid.CellToComponent<Wall>(Cell.GetParallelEdge(cell, cap), tile.layer);

        private Wall GetPerpendicular (Cell cell, CellEdgeSide capSide, CellEdgeSide perpendicularSide) =>
            puzzle.grid.CellToComponent<Wall>(Cell.PerpendicularEdge(cell, capSide, perpendicularSide), tile.layer);

        private bool IsHigherPriority (Wall compare)
        {
            if (compare == null)
                return true;

            if (_capPriority > compare._capPriority)
                return true;

            if (_capPriority < compare._capPriority)
                return false;

            // Priority is the same so use the edge to choose one
            var cell = tile.cell;
            var compareCell = compare.tile.cell;

            if (cell.edge > compareCell.edge)
                return true;

            if (cell.edge < compareCell.edge)
                return false;

            // Edge is the same so use x/y to choose one
            return cell.x > compareCell.x || cell.y > compareCell.y;
        }

        private bool UpdateCap (Cell cell, CellEdgeSide cap, bool updateNeighbors)
        {
            var wallParallel = GetParallel(cell, cap);
            var wallPerpendicular0 = GetPerpendicular(cell, cap, CellEdgeSide.Min);
            var wallPerpendicular1 = GetPerpendicular(cell, cap, CellEdgeSide.Max);

            if(updateNeighbors)
            {
                if (wallParallel != null)
                    wallParallel.UpdateVisuals(wallParallel.tile.cell, false);
                if (wallPerpendicular0 != null)
                    wallPerpendicular0.UpdateVisuals(wallPerpendicular0.tile.cell, false);
                if (wallPerpendicular1 != null)
                    wallPerpendicular1.UpdateVisuals(wallPerpendicular1.tile.cell, false);
            }

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

            // Rotate wall when on the east edge
            if(cell.edge == CellEdge.East)
                _visuals.transform.localEulerAngles = new Vector3(0, 90, 0);

            // Update the caps
            var capLeft = UpdateCap(cell, CellEdgeSide.Min, updateNeighbors);
            var capRight = UpdateCap(cell, CellEdgeSide.Max, updateNeighbors);

            // Show/Hide the caps if there are any
            if (_capLeft != null)
                _capLeft.gameObject.SetActive(capLeft);

            if (_capRight != null)
                _capRight.gameObject.SetActive(capRight);
        }

        public Tile[] GetMountedTiles ()
        {
            var tile0 = tile.grid.CellToTile(tile.cell.ConvertTo(CellCoordinateSystem.Edge), TileLayer.WallStatic);
            var tile1 = tile.grid.CellToTile(
                tile.cell.edge == CellEdge.East ?
                    new Cell(tile.cell + new Vector2Int(1, 0), CellEdge.West) :
                    new Cell(tile.cell + new Vector2Int(0, 1), CellEdge.South), 
                TileLayer.WallStatic);

            if (tile0 != null && tile1 != null)
                return new Tile[] { tile0, tile1 };
            else if (tile0 != null)
                return new Tile[] { tile0 };
            else if (tile1 != null)
                return new Tile[] { tile1 };

            return new Tile[] { };
        }
    }
}
