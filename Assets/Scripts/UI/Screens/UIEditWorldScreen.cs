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
        [SerializeField] private Button _playWorldButton = null;
        [SerializeField] private Button _exportWorldButton = null;
        [SerializeField] private Button _worldOptionsButton = null;
        [SerializeField] private Button _puzzleOptionsButton = null;
        [SerializeField] private ScrollRect _scrollRect = null;

        [SerializeField] private UIPuzzleList _puzzleList = null;
        [SerializeField] private UIPuzzleListItem _puzzleListItemPrefab = null;
        [SerializeField] private TMPro.TextMeshProUGUI _worldNameText = null;

        [SerializeField] private UIInputBlocker _puzzlePopup = null;
        [SerializeField] private RectTransform _puzzlePopupButtons = null;

        private World _world;
        public World world {
            get => _world;
            set => _world = value;
        }

        private void Awake()
        {
            _puzzleOptionsButton.onClick.AddListener(() => {
                //UIManager.ShowPuzzleOptionsScreen();
            });

            _puzzlePopup.onCancel.AddListener(() => {
                _puzzlePopup.gameObject.SetActive(false);
            });

            _closeButton.onClick.AddListener(() => {
                UIManager.ShowCreateScreen();
            });

            _puzzleList.onSelectionChanged += (selection) => UpdateButtons();
            _puzzleList.onReorderItem += OnPuzzleListReorderItem;

            _worldOptionsButton.onClick.AddListener(() => {
                UIManager.ShowEditWorldPropertiesScreen(_world);
            });

            _editPuzzleButton.onClick.AddListener(() => {
                HidePuzzlePopup();
                Editor.UIPuzzleEditor.Initialize((_puzzleList.selectedItem as UIPuzzleListItem).puzzleEntry);
            });

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
                HidePuzzlePopup();
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
                HidePuzzlePopup();
                var puzzleEntry = (_puzzleList.selectedItem as UIPuzzleListItem).puzzleEntry;
                puzzleEntry = _world.DuplicatePuzzleEntry(puzzleEntry);
                UpdateWorld();
                Select(puzzleEntry);
            });

            _playWorldButton.onClick.AddListener(() =>
            {
                UIManager.EnterPlayWorldScreen(world, isDebugging : true);
            });

            _exportWorldButton.onClick.AddListener(() =>
            {
                if (_world != null)
                {
                    _world.Export();

                    UIManager.ShowConfirmPopup(
                        message: "World Exported.",
                        title: "Export",
                        confirm: "Ok");
                }
            });

            _deletePuzzleButton.onClick.AddListener(() => {
                HidePuzzlePopup();
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

        private void OnPuzzleListReorderItem(int fromIndex, int toIndex)
        {
            var puzzleEntry = _world.GetPuzzleEntry(fromIndex);
            _world.SetPuzzleEntryIndex(puzzleEntry, toIndex);
            _puzzleList.ClearSelection();
            _puzzleList.Select(toIndex);

            for (int i = 0; i < _puzzleList.itemCount; i++)
                (_puzzleList.GetItem(i) as UIPuzzleListItem).UpdateIndex();
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
            if (null != _world && _world.isModified)
                _world.Save();
        }

        private void UpdateWorld ()
        {
            if (_world == null || !isActiveAndEnabled)
                return;

            _worldNameText.text = _world.displayName;

            _puzzleList.transform.DetachAndDestroyChildren();

            foreach (var entry in _world.puzzles)
            {
                var item = Instantiate(_puzzleListItemPrefab.gameObject, _puzzleList.transform).GetComponent<UIPuzzleListItem>();
                item.puzzleEntry = entry;
                item.onDoubleClick.AddListener(() => {
                    Editor.UIPuzzleEditor.Initialize(entry);
                });
                item.onDrawerClick.AddListener((rect) => {
                    Select(entry);
                    _puzzlePopup.gameObject.SetActive(true);
                    var bounds = rect.TransformBoundsTo(_puzzlePopup.transform);
                    var prect = _puzzlePopup.GetComponent<RectTransform>().rect;
                    _puzzlePopupButtons.anchorMin = _puzzlePopupButtons.anchorMax = 
                    new Vector2(bounds.min.x / prect.width, bounds.center.y / prect.height);
                });
            }

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            var selected = _puzzleList.selected != -1;
            _editPuzzleButton.interactable = selected;
            _duplicatePuzzleButton.interactable = selected;
            _renamePuzzleButton.interactable = selected;
            _deletePuzzleButton.interactable = selected;
            _worldOptionsButton.interactable = true;
            _playWorldButton.interactable = true;
            _exportWorldButton.interactable = true;
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

        private void HidePuzzlePopup () => _puzzlePopup.gameObject.SetActive(false);
    }
}
