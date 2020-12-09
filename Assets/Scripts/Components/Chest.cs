using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class Chest : TileComponent
    {
        private bool _isLocked = false;

        [Editable]
        public System.Guid keyItem { get; private set; }

        // TODO: these should be prefabs?
//      [SerializeField] private ItemType keyItem = ItemType.None;
        [SerializeField] private ItemType prizeItem = ItemType.None;

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
        private void OnQueryUse(QueryUseEvent evt)
        {
            // chest can only be used if it is unlocked
            if (isLocked)
                return;

#if false
            if (keyItem != ItemType.None)
            {
                // check if the using actor has the keyItem
                QueryHasItemEvent hasItemEvent = new QueryHasItemEvent(keyItem);
                evt.source.Send(hasItemEvent);

                if (hasItemEvent.result)
                {
                    // yes, we can use this lockable
                    evt.result = true;
                }
            }
#endif
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            Open();
        }

        [ActorEventHandler]
        private void OnActivateWire(WireActivatedEvent evt)
        {
            isLocked = false;
        }

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt)
        {
            isLocked = true;
        }

        private void Open()
        {
            // TODO: animate chest away and instantiate prize item
            UpdateVisuals();
        }
    }
}
