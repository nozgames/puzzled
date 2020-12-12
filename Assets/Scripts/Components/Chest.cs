using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Chest : TileComponent
    {
        private bool _isLocked = false;

        [Editable]
        public System.Guid keyItem { get; private set; }

        [Editable]
        public System.Guid prizeItem { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject lockedVisual;
        [SerializeField] private GameObject unlockedVisual;

        [Editable]
        public bool isLocked
        {
            get => _isLocked;
            set
            {
                _isLocked = value;
                UpdateVisuals();
            }
        }

        private void Start()
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            lockedVisual.SetActive(isLocked);
            unlockedVisual.SetActive(!isLocked);
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            // Always retport we were used, even if the use fails
            evt.IsHandled = true;

            // Check to see if the user has the item to unlock the chest
            isLocked = (keyItem == null || !evt.user.Send(new QueryHasItemEvent(keyItem)));
            if (isLocked)
                return;

            var cell = tile.cell;

            // Destroy ourself first or the new item cannot spawn 
            tile.Destroy();

            // Spawn the prize at the same spot
            GameManager.InstantiateTile(TileDatabase.GetTile(prizeItem), cell);
        }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt) => isLocked = false;

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt) => isLocked = false;
    }
}
