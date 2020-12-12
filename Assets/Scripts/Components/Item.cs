using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Item : TileComponent
    {
        //[SerializeField] private Sprite _icon = null;
        [SerializeField] private GameObject _visuals = null;
        
        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;

            if(evt.user.Send(new GiveItemEvent(this)))
                tile.Destroy();
        }

        public GameObject CloneVisuals (Transform parent)
        {
            var cloned = Instantiate(_visuals, parent);
            cloned.GetComponent<Floating>().enabled = false;
            return cloned;
        }
    }
}
