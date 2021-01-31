using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Floor : TileComponent
    {
        [SerializeField] private Transform _visual = null;

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateVisuals();

        [ActorEventHandler]
        private void OnCellChanged(CellChangedEvent evt) => UpdateVisuals();

        private void UpdateVisuals()
        {
            if (tile.cell == Cell.invalid)
                return;

            var state = Random.state;
            Random.InitState(tile.cell.x * tile.cell.y);
            _visual.localRotation = Quaternion.Euler(0, 90.0f * Random.Range(0, 4), 0.0f);
            Random.state = state;
        }
    }
}
