using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Keypad : TileComponent
    {
        private bool isLocked = true;
        private int _value = 0;

        [Editable]
        public int value 
        {
            get => _value;
            set
            {
                _value = value;
                UpdateVisuals();
            }
        }

        [Header("Visuals")]
        [SerializeField] private GameObject visualLocked;
        [SerializeField] private GameObject visualUnlocked;

        [ActorEventHandler]
        private void OnQueryUse(QueryUseEvent evt)
        {
            evt.result = isLocked;
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            // FIXME
            // open keypad UI and get input
            // check against value

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            visualLocked.SetActive(isLocked);
            visualUnlocked.SetActive(!isLocked);
        }
    }
}
