using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Item : TileComponent
    {
        [SerializeField] private GameObject _visuals = null;
        [SerializeField] private AudioClip _pickupSound = null;

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;

            if (evt.user.Send(new GiveItemEvent(this)))
            {
                if(_pickupSound != null)
                    AudioManager.Instance.Play(_pickupSound);
                tile.Destroy();
            }
        }

        public GameObject CloneVisuals (Transform parent)
        {
            var cloned = Instantiate(_visuals, parent);
            cloned.GetComponent<Floating>().enabled = false;
            return cloned;
        }
    }
}
