using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Chest : TileComponent
    {
        [Header("General")]
        [SerializeField] private Tile keyItem = null;
        [SerializeField] private AudioClip _unlockSound = null;

        [Editable]
        public System.Guid prizeItem { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject lockedVisual;
        [SerializeField] private GameObject unlockedVisual;

        private bool locked = false;

        
        private void Start()
        {
            locked = keyItem != null;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            lockedVisual.SetActive(locked);
            unlockedVisual.SetActive(!locked);
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            // Always retport we were used, even if the use fails
            evt.IsHandled = true;

            if(locked)
            {
                // Check to see if the user has the item to unlock the chest
                locked = !evt.user.Send(new QueryHasItemEvent(keyItem.guid));
                if (locked)
                    return;

                if(_unlockSound != null)
                    AudioManager.Instance.Play(_unlockSound);
            }

            var cell = tile.cell;

            // Destroy ourself first or the new item cannot spawn 
            tile.Destroy();

            // Spawn the prize at the same spot
            puzzle.InstantiateTile(TileDatabase.GetTile(prizeItem), cell);
        }
    }
}
