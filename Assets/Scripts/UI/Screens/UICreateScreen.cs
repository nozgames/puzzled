using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UICreateScreen : UIScreen
    {
        [SerializeField] private Button _closeButton = null;

        [SerializeField] private UIWorldList _worldList = null;
        [SerializeField] private UIWorldListItem _worldListItemPrefab = null;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => {
                UIManager.ShowMainScreen();
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
                    UIManager.ShowEditWorldScreen(entry);
                });
            }
        }
    }
}
