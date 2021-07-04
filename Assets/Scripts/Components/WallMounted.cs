using NoZ;
using UnityEngine;

namespace Puzzled
{
    class WallMounted : TileComponent
    {
        [SerializeField] private Transform _visuals = null;
        [SerializeField] private float _height = 0.5f;

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

        public void UpdateVisuals()
        {
            if (tile.cell == Cell.invalid)
                return;

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

            // Do not adjust the height if in preview mode
            if (puzzle.isPreview)
                return;

            var wall = tile.grid.CellToComponent<Wall>(tile.cell.ConvertTo(CellCoordinateSystem.SharedEdge), TileLayer.Wall);
            if (wall != null)
                _visuals.position = wall.tile.grid.CellToWorldBounds(wall.tile.cell).center + _visuals.forward * wall.thickness;

            _visuals.position += Vector3.up * _height;
        }
    }
}
