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
        private void OnQueryUse(QueryUseEvent evt)
        {
            if (keyItem != null)
            {
                // check if the using actor has the keyItem
                QueryHasItemEvent hasItemEvent = new QueryHasItemEvent(keyItem);
                evt.source.Send(hasItemEvent);

                if (hasItemEvent.IsHandled)
                {
                    isLocked = false;
                }
            }

            // chest can only be used if it is unlocked
            if (isLocked)
                return;

            GameManager.InstantiateTile(TileDatabase.GetTile(prizeItem), tile.cell);
            Destroy(tile.gameObject);
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
