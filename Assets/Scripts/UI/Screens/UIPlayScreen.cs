using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIPlayScreen : UIScreen
    {
        [SerializeField] private UIWorldList _worldList = null;
        [SerializeField] private UIWorldListItem _worldListItemPrefab = null;

        override public bool showConfirmButton => true;
        override public string confirmButtonText => "Enter World";
        override public bool showCancelButton => true;

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

            _worldList.Select();
            _worldList.SelectItem(0);
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
