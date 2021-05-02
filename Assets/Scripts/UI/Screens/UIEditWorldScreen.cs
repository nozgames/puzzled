using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIEditWorldScreen : UIScreen
    {
        [SerializeField] private Button _closeButton = null;
        [SerializeField] private Button _newPuzzleButton = null;
        [SerializeField] private Button _editPuzzleButton = null;
        [SerializeField] private Button _renamePuzzleButton = null;
        [SerializeField] private Button _deletePuzzleButton = null;
        [SerializeField] private Button _duplicatePuzzleButton = null;
        [SerializeField] private ScrollRect _scrollRect = null;

        [SerializeField] private UIPuzzleList _puzzleList = null;
        [SerializeField] private UIPuzzleListItem _puzzleListItemPrefab = null;
        [SerializeField] private TMPro.TextMeshProUGUI _worldNameText = null;       

        private World _world;

        public World world {
            get => _world;
            set {
                _world = value;
                UpdateWorld();
            }
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => {
                UIManager.ShowCreateScreen();
            });

            _puzzleList.onSelectionChanged += (selection) => UpdateButtons();

            _newPuzzleButton.onClick.AddListener(() => {
                UIManager.ShowNamePopup("", title: "New Puzzle", commit: "Create", placeholder: "Enter Puzzle Name",
                    onCommit: (name) => {
                        var error = ValidateName(name);
                        if (error != null)
                            return error;

                        var puzzleEntry = _world.NewPuzzleEntry(name);
                        if (null == puzzleEntry)
                            return "Error: Failed to create puzzle";

                        Editor.UIPuzzleEditor.Initialize(puzzleEntry);

                        return null;
                    }
                );
            });

            _renamePuzzleButton.onClick.AddListener(() => {
                var puzzleEntry = (_puzzleList.selectedItem as UIPuzzleListItem).puzzleEntry;
                UIManager.ShowNamePopup(
                    puzzleEntry.name,
                    title: "Rename Puzzle", commit: "Rename", placeholder: "Enter Puzzle Name",
                    onCommit: (name) => {
                        var error = ValidateName(name);
                        if (error != null)
                            return error;

                        _world.RenamePuzzleEntry(puzzleEntry, name);
                        UpdateWorld();
                        return null;
                    }
                );
            });

            _duplicatePuzzleButton.onClick.AddListener(() => {
                var puzzleEntry = (_puzzleList.selectedItem as UIPuzzleListItem).puzzleEntry;
                puzzleEntry = _world.DuplicatePuzzleEntry(puzzleEntry);
                UpdateWorld();
                Select(puzzleEntry);
            });

            _deletePuzzleButton.onClick.AddListener(() => {
                UIManager.ShowConfirmPopup(
                    message: "Are you sure you want to delete this puzzle?",
                    title: "Delete Puzzle",
                    confirm: "Delete",
                    onConfirm: () => {
                        var puzzleEntry = (_puzzleList.selectedItem as UIPuzzleListItem).puzzleEntry;
                        var index = _puzzleList.selected;
                        _world.DeletePuzzleEntry(puzzleEntry);
                        UpdateWorld();

                        index = Mathf.Min(_puzzleList.itemCount - 1, index);
                        if (index != -1)
                            Select((_puzzleList.GetItem(index) as UIPuzzleListItem).puzzleEntry);
                    }
                    );
            });
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
            UpdateButtons();
        }

        private void UpdateWorld ()
        {
            if (_world == null || !isActiveAndEnabled)
                return;

            _worldNameText.text = _world.displayName;

            _puzzleList.transform.DetachAndDestroyChildren();

            foreach (var entry in _world.puzzles.OrderBy(e => e.name))
            {
                var item = Instantiate(_puzzleListItemPrefab.gameObject, _puzzleList.transform).GetComponent<UIPuzzleListItem>();
                item.puzzleEntry = entry;
                item.onDoubleClick.AddListener(() => {
                    Editor.UIPuzzleEditor.Initialize(entry);
                });
            }
        }

        private void UpdateButtons()
        {
            var selected = _puzzleList.selected != -1;
            _editPuzzleButton.interactable = selected;
            _duplicatePuzzleButton.interactable = selected;
            _renamePuzzleButton.interactable = selected;
            _deletePuzzleButton.interactable = selected;
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
