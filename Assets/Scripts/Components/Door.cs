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
        [SerializeField] private ItemType keyItem = ItemType.None;

        [Header("Visuals")]
        [SerializeField] private GameObject openedVisual;
        [SerializeField] private GameObject closedVisual;

        private void Start()
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            openedVisual.SetActive(!isClosed);
            closedVisual.SetActive(isClosed);
        }

        [ActorEventHandler(priority=1)]
        private void OnQueryMove(QueryMoveEvent evt)
        {
            evt.result = !isClosed;
            evt.IsHandled = true;
        }

        [ActorEventHandler]
        private void OnQueryUse(QueryUseEvent evt)
        {
            // door can only be used if it is still closed 
            if (!isClosed)
                return;

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
            Open();            
        }

        public void Open()
        {
            isClosed = false;
            UpdateVisuals();
        }

        public void Close()
        {
            isClosed = true;
            UpdateVisuals();
        }
    }
}
