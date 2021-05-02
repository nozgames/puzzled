using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIEditWorldScreen : UIScreen
    {
        [SerializeField] private Button _closeButton = null;
        [SerializeField] private Button _newPuzzleButton = null;

        [SerializeField] private UIPuzzleList _puzzleList = null;
        [SerializeField] private UIPuzzleListItem _puzzleListItemPrefab = null;
        [SerializeField] private TMPro.TextMeshProUGUI _worldNameText = null;

        [SerializeField] private GameObject _newPuzzlePopup = null;
        [SerializeField] private TMPro.TMP_InputField _newPuzzleNameField = null;
        [SerializeField] private Button _newPuzzleOkButton = null;
        [SerializeField] private Button _newPuzzleCloseButton = null;
        [SerializeField] private TMPro.TextMeshProUGUI _newPuzzleErrorText = null;

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

            _newPuzzleButton.onClick.AddListener(() => {
                _newPuzzleErrorText.gameObject.SetActive(false);
                _newPuzzleNameField.text = "";
                _newPuzzlePopup.gameObject.SetActive(true);
                _newPuzzleNameField.Select();
            });

            _newPuzzleCloseButton.onClick.AddListener(() => {
                _newPuzzlePopup.gameObject.SetActive(false);
            });

            _newPuzzleOkButton.onClick.AddListener(() => {
                var puzzleEntry = _world.NewPuzzleEntry(_newPuzzleNameField.text);
                if(null == puzzleEntry)
                {
                    _newPuzzleErrorText.gameObject.SetActive(true);
                    return;
                }
                Editor.UIPuzzleEditor.Initialize(puzzleEntry);
            });

            _newPuzzleErrorText.gameObject.SetActive(false);
        }

        private void OnEnable()
        {            
            _newPuzzlePopup.gameObject.SetActive(false);
            UpdateWorld();
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
    }
}
