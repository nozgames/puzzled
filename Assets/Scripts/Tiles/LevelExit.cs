using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class LevelExit : TileComponent
    {
        [SerializeField] private GameObject _visuals = null;
        //[SerializeField] private AudioClip _pickupSound = null;

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            evt.user.Send(new LevelExitEvent(this));
            tile.Destroy();
        }

        public GameObject CloneVisuals(Transform parent)
        {
            var cloned = Instantiate(_visuals, parent);
            cloned.GetComponent<Floating>().enabled = false;
            return cloned;
        }
    }
}
