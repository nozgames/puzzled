using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIChoosePuzzlePopup : MonoBehaviour
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

            if(!_save.activeSelf)
            {
                var recent = Instantiate(_itemPrefab, _worlds.transform).GetComponent<UIChoosePuzzlePopupItem>();
                recent.text = "Recent";
                recent.data = "";
            }

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

            if(string.IsNullOrWhiteSpace(world))
            {
                for (int i = 0; i < 10; i++)
                {
                    var recentPath = UnityEngine.PlayerPrefs.GetString($"Puzzle.Recent{i}");
                    if (!File.Exists(recentPath))
                        continue;

                    var recentWorld = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(recentPath));
                    if (!Directory.Exists(Path.Combine(Application.dataPath, "Puzzles", recentWorld)))
                        continue;

                    var item = Instantiate(_itemPrefab, _puzzles.transform).GetComponent<UIChoosePuzzlePopupItem>();
                    item.data = recentPath;
                    item.text = $"{recentWorld} / {Path.GetFileNameWithoutExtension(recentPath)}";
                }
            }
            else
            {
                var files = Directory.GetFiles(world, "*.puzzle");
                foreach (var file in files)
                {
                    var item = Instantiate(_itemPrefab, _puzzles.transform).GetComponent<UIChoosePuzzlePopupItem>();
                    item.data = file;
                    item.text = Path.GetFileNameWithoutExtension(file);
                }
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
            _title.text = string.IsNullOrWhiteSpace(filename) ? 
                "Save Puzzle" : "Save Puzzle As";

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

        private void UpdateRecent(string path)
        {
            // If already in there then remove it
            for (int i = 0; i<10; i++)
                if(PlayerPrefs.GetString($"Puzzle.Recent{i}") == path)
                {
                    for (int j = i; j < 9; j++)
                        PlayerPrefs.SetString($"Puzzle.Recent{j}", PlayerPrefs.GetString($"Puzzle.Recent{j + 1}"));

                    PlayerPrefs.DeleteKey($"Puzzle.Recent{9}");
                }

            // Shift all recents down by one
            for (int i=9; i>0; i--)
                PlayerPrefs.SetString($"Puzzle.Recent{i}", PlayerPrefs.GetString($"Puzzle.Recent{i-1}"));

            // Add at start
            PlayerPrefs.SetString($"Puzzle.Recent0", path);
        }

        private void Done()
        {
            if (!_save.activeSelf)
            {
                var path = ((UIChoosePuzzlePopupItem)_puzzles.selectedItem)?.data;
                if (null == path)
                    return;

                UpdateRecent(path);

                onOpenPuzzle?.Invoke(path);
            } 
            else
            {
                var path = Path.Combine(((UIChoosePuzzlePopupItem)_worlds.selectedItem).data, $"{_saveFilename.text}.puzzle");
                UpdateRecent(path);
                onSaveFile?.Invoke(path);
            }                
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
