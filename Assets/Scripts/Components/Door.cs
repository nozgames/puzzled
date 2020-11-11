using NoZ;
using Puzzled;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puzzled
{
    public class Door : ActorComponent
    {
        [Header("Logic")]
        [SerializeField] private bool isClosed = true;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private ItemType keyItem = ItemType.None;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer visuals;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite lockedSprite;

        private void Start()
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            visuals.sprite = isClosed ? (isLocked ? lockedSprite : closedSprite) : openSprite;
        }

        [ActorEventHandler]
        private void OnQueryMove(QueryMoveEvent evt)
        {
            evt.result = !isClosed;
        }

        [ActorEventHandler]
        private void OnQueryUse(QueryUseEvent evt)
        {
            // door can only be used if it is still closed 
            if (!isClosed)
                return;

            if (!isLocked)
            {
                // we can open it
                evt.result = true;
                return;
            }

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
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            if (isLocked)
            {
                Unlock();
                return;
            }

            isClosed = false;
            UpdateVisuals();
        }

        public void Unlock()
        {
            isLocked = false;
            UpdateVisuals();
        }

        public void Lock()
        {
            if (!isClosed)
                return;

            isLocked = true;
            UpdateVisuals();
        }
    }
}
