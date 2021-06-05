using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Puzzled.UI
{
    class UIPuzzleListItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
        [SerializeField] private TMPro.TextMeshProUGUI _indexText = null;
        [SerializeField] private Image _finishedImage = null;
        [SerializeField] private Image _lockedImage = null;

        [SerializeField] private Button _drawerButton = null;

        public UnityEvent<RectTransform> onDrawerClick;

        private World.IPuzzleEntry _puzzleEntry;

        public World.IPuzzleEntry puzzleEntry {
            get => _puzzleEntry;
            set {
                _puzzleEntry = value;
                UpdatePuzzleEntry();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (_drawerButton != null)
                _drawerButton.onClick.AddListener(() => {
                    onDrawerClick?.Invoke(_drawerButton.GetComponent<RectTransform>());
                });
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

            if (_lockedImage != null)
                _lockedImage.gameObject.SetActive(_puzzleEntry.isLocked);

            if (_finishedImage != null)
                _finishedImage.gameObject.SetActive(_puzzleEntry.isCompleted && (null == _lockedImage || !_lockedImage.gameObject.activeSelf));

            UpdateIndex();
        }

        public void UpdateIndex ()
        {
            if (_indexText != null)
                _indexText.text = (transform.GetSiblingIndex() + 1).ToString();
        }
    }
}
