using NoZ;
using UnityEngine;

namespace Puzzled
{
    class WallMounted : TileComponent
    {
        [SerializeField] private Transform _visuals = null;

        [ActorEventHandler]
        private void OnStartEvent (StartEvent evt)
        {
            UpdateVisuals();
        }

        [ActorEventHandler]
        private void OnCellChangedEvent (CellChangedEvent evt)
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            var wall = tile.grid.CellToComponent<Wall>(tile.cell.ConvertTo(CellCoordinateSystem.SharedEdge), TileLayer.Wall);
            if (wall != null)
                _visuals.position = wall.tile.grid.CellToWorldBounds(wall.tile.cell).center + _visuals.forward * wall.thickness;
            else
                _visuals.position = transform.position + Vector3.up * 0.5f;

            switch (tile.cell.edge)
            {
                default:
                case CellEdge.North:
                    _visuals.rotation = Quaternion.LookRotation(-Vector3.forward, Vector3.up);
                    break;

                case CellEdge.South:
                    _visuals.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                    break;

                case CellEdge.East:
                    _visuals.rotation = Quaternion.LookRotation(-Vector3.right, Vector3.up);
                    break;

                case CellEdge.West:
                    _visuals.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
                    break;
            }

        }
    }
}
