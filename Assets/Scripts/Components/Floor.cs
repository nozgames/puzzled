using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Floor : TileComponent
    {
        [SerializeField] private GameObject visualDk;
        [SerializeField] private GameObject visualLt;
        [SerializeField] private SpriteRenderer _gradient = null;

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateVisuals();

        [ActorEventHandler]
        private void OnCellChanged(CellChangedEvent evt) => UpdateVisuals();

        private void UpdateVisuals()
        {
            if (tile.cell == Cell.invalid)
                return;

            _gradient.sharedMaterial = CameraManager.floorGradientMaterial;

            var dark = (tile.cell.y + tile.cell.x) % 2 == 0;
            visualDk.SetActive(dark);
            visualLt.SetActive(!dark);
        }
    }
}
