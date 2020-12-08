using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Item : TileComponent
    {
        [SerializeField] private GameObject _visuals = null;
        [SerializeField] private GameObject _shadow = null;

        [ActorEventHandler]
        private void OnEnterCell(EnterCellEvent evt)
        {
            if (!_visuals.activeSelf)
                return;

            var giveItem = new GiveItemEvent(this);
            evt.tile.Send(giveItem);
            _visuals.SetActive(!giveItem.IsHandled);
            _shadow.SetActive(!giveItem.IsHandled);
        }

        public GameObject CloneVisuals (Transform parent)
        {
            var cloned = Instantiate(_visuals, parent);
            cloned.GetComponent<Floating>().enabled = false;
            return cloned;
        }
    }
}
