using NoZ;
using Puzzled;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puzzled
{
    public class Door : TileComponent
    {
        [Header("Logic")]
        private bool _isOpen = false;
        [SerializeField] private ItemType keyItem = ItemType.None;

        [Header("Visuals")]
        [SerializeField] private GameObject openedVisual;
        [SerializeField] private GameObject closedVisual;

        [Editable]
        public bool isOpen {
            get => _isOpen;
            set {
                _isOpen = value;
                UpdateVisuals();
            }
        }

        private void Start()
        {
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            openedVisual.SetActive(_isOpen);
            closedVisual.SetActive(!_isOpen);
        }

        [ActorEventHandler(priority=1)]
        private void OnQueryMove(QueryMoveEvent evt)
        {
            evt.result = _isOpen;
        }

        [ActorEventHandler]
        private void OnQueryUse(QueryUseEvent evt)
        {
            // door can only be used if it is still closed 
            if (_isOpen)
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
            Open();
        }

        [ActorEventHandler]
        private void OnDeactivateWire(WireDeactivatedEvent evt)
        {
            Close();
        }

        public void Open() => isOpen = true;
        public void Close() => isOpen = false;
    }
}
