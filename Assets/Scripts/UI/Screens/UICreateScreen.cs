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

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => {
                UIManager.ShowMainScreen();
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

                    World world = WorldManager.LoadWorld(entry);
                    UIManager.EnterEditWorldScreen(world);
                });
            }
        }
    }
}
