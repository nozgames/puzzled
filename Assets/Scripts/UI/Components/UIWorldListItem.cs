using UnityEngine;

namespace Puzzled.UI
{
    class UIWorldListItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;

        private WorldManager.IWorldEntry _worldEntry;

        public WorldManager.IWorldEntry worldEntry {
            get => _worldEntry;
            set {
                _worldEntry = value;
                UpdateWorldEntry();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            UpdateWorldEntry();
        }

        private void UpdateWorldEntry()
        {
            if (!isActiveAndEnabled || _worldEntry == null)
                return;

            if (_nameText != null)
                _nameText.text = _worldEntry.name;
        }
    }
}
