using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIChooseFilePopup : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _title = null;
        [SerializeField] private UIList _worlds = null;
        [SerializeField] private UIList _puzzles = null;
        [SerializeField] private UIListItem _itemPrefab = null;
        [SerializeField] private Button _okButton = null;
        [SerializeField] private Button _cancelButton = null;
        [SerializeField] private GameObject _save = null;
        [SerializeField] private TMPro.TMP_InputField _saveFilename = null;

        public event Action<string> onOpenPuzzle;
        public event Action<string> onSaveFile;
        public event Action onCancel;

        private void Awake()
        {
            _okButton.onClick.AddListener(Done);
            _cancelButton.onClick.AddListener(() => onCancel?.Invoke());
            _worlds.onSelectionChanged += OnWorldSelectionChanged;
            _puzzles.onSelectionChanged += OnPuzzleSelectionChanged;
            _puzzles.onDoubleClickItem += OnPuzzleDoubleClickItem;
            _saveFilename.onValueChanged.AddListener((value) => UpdateButtons());
        }

        private void OnPuzzleDoubleClickItem(int selection) => Done();

        private void OnPuzzleSelectionChanged(int selection)
        {
            if (_save.activeSelf)            
                _saveFilename.text = ((UIChoosePuzzlePopupItem)_puzzles.GetItem(selection)).text;

            UpdateButtons();
        }

        private void OnWorldSelectionChanged(int selection)
        {
            UpdatePuzzles((_worlds.selectedItem as UIChoosePuzzlePopupItem).data);
        }

        private void UpdateWorlds()
        {
            _worlds.transform.DetachAndDestroyChildren();

            var worlds = Directory.GetDirectories(Path.Combine(Application.dataPath, "Puzzles"));
            foreach (var world in worlds)
            {
                var item = Instantiate(_itemPrefab, _worlds.transform).GetComponent<UIChoosePuzzlePopupItem>();
                item.text = Path.GetFileNameWithoutExtension(world);
                item.data = world;
            }

            _worlds.Select(0);
        }

        private void UpdatePuzzles(string world)
        {
            _puzzles.transform.DetachAndDestroyChildren();

            var files = Directory.GetFiles(world, "*.puzzle");
            foreach (var file in files)
            {
                var fileDir = Path.GetDirectoryName(file);
                var item = Instantiate(_itemPrefab, _puzzles.transform).GetComponent<UIChoosePuzzlePopupItem>();
                item.data = file;
                item.text = Path.GetFileNameWithoutExtension(file);
            }

            UpdateButtons();
        }

        public void OpenPuzzle ()
        {
            _save.gameObject.SetActive(false);
            _title.text = "Open Puzzle";

            // TODO: remember the last world chosen

            UpdateWorlds();
        }

        public void SavePuzzle (string filename)
        {
            _save.gameObject.SetActive(true);
            _saveFilename.text = filename;

            UpdateWorlds();

            if(!string.IsNullOrWhiteSpace(filename))
            {
                var world = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(filename));

                if (Directory.Exists(Path.Combine(Application.dataPath, "Puzzles", world)))
                { 
                    for(int i=0; i<_worlds.itemCount; i++)
                        if(0 == string.Compare(((UIChoosePuzzlePopupItem)_worlds.GetItem(i)).text, world, false))
                        {
                            _worlds.Select(i);
                            break;
                        }
                }

                _saveFilename.text = Path.GetFileNameWithoutExtension(filename);
            }

            // Focus the save file name
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_saveFilename.gameObject);
        }

        private void Done()
        {
            if (!_save.activeSelf)
            {
                var filename = ((UIChoosePuzzlePopupItem)_puzzles.selectedItem)?.data;
                if (null == filename)
                    return;

                onOpenPuzzle?.Invoke(filename);
            } 
            else
                onSaveFile?.Invoke(Path.Combine(((UIChoosePuzzlePopupItem)_worlds.selectedItem).data, $"{_saveFilename.text}.puzzle"));
        }

        private void UpdateButtons()
        {
            if (_save.activeSelf)
                _okButton.interactable = _worlds.selected != -1 && _saveFilename.text.Length > 0;
            else
                _okButton.interactable = _puzzles.selectedItem != null;
        }
   }
}
