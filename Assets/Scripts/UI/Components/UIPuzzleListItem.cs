using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIPuzzleListItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private Image _finishedImage = null;
        [SerializeField] private TMPro.TextMeshProUGUI _indexText = null;

        private World.IPuzzleEntry _puzzleEntry;

        public World.IPuzzleEntry puzzleEntry {
            get => _puzzleEntry;
            set {
                _puzzleEntry = value;
                UpdatePuzzleEntry();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            UpdatePuzzleEntry();
        }

        private void UpdatePuzzleEntry()
        {
            if (!isActiveAndEnabled || _puzzleEntry == null)
                return;

            if (_nameText != null)
                _nameText.text = _puzzleEntry.name;

            if (_finishedImage != null)
                _finishedImage.enabled = _puzzleEntry.isCompleted;

            UpdateIndex();
        }

        public void UpdateIndex ()
        {
            if (_indexText != null)
                _indexText.text = (transform.GetSiblingIndex() + 1).ToString();
        }
    }
}
