using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Puzzled.WorldManager;

namespace Puzzled.UI
{
    class UIPlayWorldScreen : UIScreen
    {
        [SerializeField] private Button _closeButton = null;
        [SerializeField] private ScrollRect _scrollRect = null;

        [SerializeField] private UIPuzzleList _puzzleList = null;
        [SerializeField] private UIPuzzleListItem _puzzleListItemPrefab = null;
        [SerializeField] private TMPro.TextMeshProUGUI _worldNameText = null;

        private World _world;
        public World world
        {
            get => _world;
            set => _world = value;
        }

        public bool isDebugging;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => {
                if (isDebugging)
                    UIManager.ShowCreateScreen();
                else
                    UIManager.ShowPlayScreen();
            });

            _puzzleList.onSelectionChanged += (selection) => UpdateButtons();
        }

        private string ValidateName (string name)
        {
            if (name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) != -1)
                return "Error: Name contains invalid characters";

            if(_world.Contains(name))
                return "Error: Puzzle with the same name already exists";

            return null;
        }

        private void OnEnable()
        {            
            UpdateWorld();            
        }

        private void OnDisable()
        {
        }

        private void UpdateWorld ()
        {
            if (_world == null || !isActiveAndEnabled)
                return;

            _worldNameText.text = _world.displayName;

            _puzzleList.transform.DetachAndDestroyChildren();

            foreach (var entry in _world.puzzles)
            {
                var locked = entry.isLocked;
                if (locked && entry.hideWhenLocked)
                    continue;

                var item = Instantiate(_puzzleListItemPrefab.gameObject, _puzzleList.transform).GetComponent<UIPuzzleListItem>();
                item.puzzleEntry = entry;

                if(!locked)
                    item.onDoubleClick.AddListener(() => {
                        // Load the puzzle and play
                        GameManager.LoadPuzzle(item.puzzleEntry);
                        GameManager.Play();

                        UIManager.HideMenu();
                    });
            }

            UpdateButtons();
        }

        private void UpdateButtons()
        {
        }

        private void Select(World.IPuzzleEntry puzzleEntry)
        {
            for(int i=0; i<_puzzleList.itemCount; i++)
            {
                var puzzleItem = _puzzleList.GetItem(i) as UIPuzzleListItem;
                if(puzzleItem.puzzleEntry == puzzleEntry)
                {
                    _puzzleList.Select(i);
                    _scrollRect.ScrollTo(puzzleItem .GetComponent<RectTransform>());
                }
            }
        }
    }
}
