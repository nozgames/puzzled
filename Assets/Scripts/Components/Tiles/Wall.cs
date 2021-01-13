using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Wall : TileComponent
    {
        [SerializeField] private GameObject _visualWest = null;
        [SerializeField] private GameObject _visualNorth = null;

        private Wall GetWall(Cell cell) => puzzle.grid.CellToComponent<Wall>(cell, TileLayer.Static);

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateVisuals();

            var east = GetWall(tile.cell + Cell.right);
            if (east != null)
                east.UpdateVisuals();

            var south = GetWall(tile.cell + Cell.down);
            if (south != null)
                south.UpdateVisuals();
        }

        private void UpdateVisuals()
        { 
            _visualWest.SetActive(GetWall(tile.cell + Cell.left) != null);
            _visualNorth.SetActive(GetWall(tile.cell + Cell.up) != null);
        }
    }
}
