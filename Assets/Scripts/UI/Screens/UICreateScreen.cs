using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UICreateScreen : UIScreen
    {
        [SerializeField] private Button _closeButton = null;
        [SerializeField] private Button _newWorldButton = null;

        [SerializeField] private UIWorldList _worldList = null;
        [SerializeField] private UIWorldListItem _worldListItemPrefab = null;
        [SerializeField] private ScrollRect _worldListScroll = null;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => {
                ExitScreen();
            });

            _newWorldButton.onClick.AddListener(() => {
                UIManager.ShowNamePopup("", title: "New World", commit: "Create", placeholder: "Enter World Name...",
                    onCommit: (value) => {
                        if (name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
                            return "Error: Name contains invalid characters";

                        var worldEntry = WorldManager.NewWorld(value);
                        if(null == worldEntry)
                            return "Error: World with the same name already exists";

                        World world = WorldManager.LoadWorld(worldEntry);
                        UIManager.EnterEditWorldScreen(world);

                        return null;
                    });
            });
        }

        private void OnEnable()
        {
            _worldList.transform.DetachAndDestroyChildren();

            foreach(var entry in WorldManager.GetEditableWorldEntries())
            {
                var item = Instantiate(_worldListItemPrefab.gameObject, _worldList.transform).GetComponent<UIWorldListItem>();
                item.worldEntry = entry;
                item.onDoubleClick.AddListener(() => {

                    EditWorld(item);
                });
            }

            _worldList.Select(0);
        }

        private void EditWorld(UIWorldListItem item)
        {
            World world = WorldManager.LoadWorld(item.worldEntry);
            UIManager.EnterEditWorldScreen(world);
        }

        private void ExitScreen()
        {
            UIManager.ShowMainScreen();
        }

        public override void HandleCancelInput()
        {
            ExitScreen();
        }

        public override void HandleUpInput()
        {
            int newSelection = Mathf.Max(_worldList.selected - 1, 0);
            _worldList.Select(newSelection);

            _worldListScroll.ScrollTo(_worldList.selectedItem.GetComponent<RectTransform>());
        }

        public override void HandleDownInput()
        {
            int newSelection = Mathf.Min(_worldList.selected + 1, _worldList.itemCount - 1);
            _worldList.Select(newSelection);

            _worldListScroll.ScrollTo(_worldList.selectedItem.GetComponent<RectTransform>());
        }

        public override void HandleConfirmInput()
        {
            UIWorldListItem item = _worldList.GetItem(_worldList.selected) as UIWorldListItem;
            EditWorld(item);
        }
    }
}
