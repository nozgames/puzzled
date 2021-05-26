using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIPlayScreen : UIScreen
    {
        [SerializeField] private Button _closeButton = null;

        [SerializeField] private UIWorldList _worldList = null;
        [SerializeField] private UIWorldListItem _worldListItemPrefab = null;
        [SerializeField] private ScrollRect _worldListScroll = null;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => {
                ExitScreen();
            });

            _worldList.onSelectionChanged += HandleSelectionChange;
        }

        private void HandleSelectionChange(int obj)
        {
            _worldListScroll.ScrollTo(_worldList.selectedItem.GetComponent<RectTransform>());
        }

        private void OnEnable()
        {
            _worldList.transform.DetachAndDestroyChildren();

            foreach (var entry in WorldManager.GetPlayableWorldEntries())
            {
                var item = Instantiate(_worldListItemPrefab.gameObject, _worldList.transform).GetComponent<UIWorldListItem>();
                item.worldEntry = entry;
                item.onDoubleClick.AddListener(() =>
                {
                    PlayWorld(item);
                });
            }

            _worldList.SelectItem(0);
            _worldList.Select();
        }

        private void PlayWorld(UIWorldListItem item)
        {
            World world = WorldManager.LoadWorld(item.worldEntry);
            world.MarkPlayed();
            UIManager.EnterPlayWorldScreen(world);
        }

        private void ExitScreen()
        {
            UIManager.ShowMainScreen();
        }

        public override void HandleCancelInput()
        {
            ExitScreen();
        }

        public override void HandleConfirmInput()
        {
            UIWorldListItem item = _worldList.GetItem(_worldList.selected) as UIWorldListItem;
            PlayWorld(item);
        }
    }
}
