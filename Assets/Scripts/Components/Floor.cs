using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Floor : TileComponent
    {
        [SerializeField] private GameObject visualDk;
        [SerializeField] private GameObject visualLt;

        [ActorEventHandler]
        private void OnCellChanged(CellChangedEvent evt)
        {
            var dark = (tile.cell.y + tile.cell.x) % 2 == 0;
            visualDk.SetActive(dark);
            visualLt.SetActive(!dark);
        }
    }
}
