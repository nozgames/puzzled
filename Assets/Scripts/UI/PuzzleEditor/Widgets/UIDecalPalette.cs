using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    class UIDecalPalette : MonoBehaviour
    {
        private readonly Regex LetterRegex = new Regex("(Letter).*", RegexOptions.IgnoreCase);
        private readonly Regex NumberRegex = new Regex("(Number).*", RegexOptions.IgnoreCase);
        private readonly Regex RuneRegex = new Regex("(Rune).*", RegexOptions.IgnoreCase);
        private readonly Regex LineRegex = new Regex("(Arrow).*", RegexOptions.IgnoreCase);

        [SerializeField] private UIDecalPaletteItem _itemPrefab = null;
        [SerializeField] private UIList _list = null;
        [SerializeField] private ScrollRect _scrollRect = null;
        [SerializeField] private Image _selectedPreview = null;
        [SerializeField] private TMPro.TextMeshProUGUI _selectedName = null;
        [SerializeField] private TMPro.TMP_InputField _searchInput = null;
        [SerializeField] private Button _searchClearButton = null;
        [SerializeField] private bool allowNone = false;

        [SerializeField] private UIRadio _filterAll = null;
        [SerializeField] private UIRadio _filterLetter = null;
        [SerializeField] private UIRadio _filterRune = null;
        [SerializeField] private UIRadio _filterNumber = null;
        [SerializeField] private UIRadio _filterLine = null;

        [SerializeField] private UIRadio _flagRotate = null;
        [SerializeField] private UIRadio _flagFlipX = null;
        [SerializeField] private UIRadio _flagFlipY = null;

        private Decal _selected;

        public Decal selected {
            get => _selected;
            set => SetSelected(value, true);
        }

        public event Action<Decal> onDoubleClickDecal;

        private void Awake()
        {
            _flagRotate.onValueChanged.AddListener((v) => { _selected.rotate = v; UpdatePreview();  });
            _flagFlipX.onValueChanged.AddListener((v) => { _selected.flipHorizontal = v; UpdatePreview(); });
            _flagFlipY.onValueChanged.AddListener((v) => { _selected.flipVertical = v; UpdatePreview(); });

            _list.onDoubleClickItem += (index) => {
                onDoubleClickDecal?.Invoke(_list.GetItem(index).GetComponent<UIDecalPaletteItem>().decal);
            };

            _list.onSelectionChanged += (index) => {
                SetSelected(_list.selectedItem?.GetComponent<UIDecalPaletteItem>().decal ?? Decal.none, false);
                UpdatePreview();
            };

            _searchClearButton.onClick.AddListener(() => {
                _searchInput.text = "";
                EventSystem.current.SetSelectedGameObject(_searchInput.gameObject);
            });
            _searchClearButton.gameObject.SetActive(false);

            _searchInput.onValueChanged.AddListener((text) => {
                _searchClearButton.gameObject.SetActive(!string.IsNullOrEmpty(text));
                UpdateFilter();
            });

            // Add a none decal
            if(allowNone)
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = Decal.none;

            // Add all decals to the palette
            foreach (var decal in DecalDatabase.GetDecals())
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = decal;

            _filterAll.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterNumber.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterRune.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterLetter.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterLine.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
        }

        private void OnEnable()
        {
            if (_list.selected == -1)
                _list.Select(0);

            UpdatePreview();
        }

        private void SetSelected (Decal value, bool scroll)
        {
            _selected = value;

            _flagFlipX.SetIsOnWithoutNotify(_selected.flipHorizontal);
            _flagFlipY.SetIsOnWithoutNotify(_selected.flipVertical);
            _flagRotate.SetIsOnWithoutNotify(_selected.rotate);

            for (int i = _list.itemCount - 1; i >= 0; i--)
            {
                var item = GetItem(i);
                if (item.decal.guid == _selected.guid && !item.selected)
                {
                    item.selected = true;
                    if (scroll && item.gameObject.activeSelf)
                        _scrollRect.ScrollTo(item.GetComponent<RectTransform>());
                    break;
                }
            }

            UpdatePreview();
        }

        private void UpdateFilter()
        {
            var checkText = _searchInput.text.Length > 0;
            var text = _searchInput.text.ToLower();
            var regex = (Regex)null;
            if (_filterNumber.isOn)
                regex = NumberRegex;
            else if (_filterRune.isOn)
                regex = RuneRegex;
            else if (_filterLetter.isOn)
                regex = LetterRegex;
            else if (_filterLine.isOn)
                regex = LineRegex;

            for (int i = allowNone ? 1 : 0; i < _list.itemCount; i++)
            {
                var item = _list.GetItem(i).GetComponent<UIDecalPaletteItem>();
                var decal = item.decal;
                var spriteName = decal.sprite != null ? decal.sprite.name : "None";

                var active = true;
                active &= !checkText || (spriteName.ToLower().Contains(text));
                active &= (regex == null) || (regex.Match(spriteName).Success);

                _list.GetItem(i).gameObject.SetActive(active);
            }
        }

        public void FilterAll() => _filterAll.isOn = true;

        public void FilterLetters() => _filterLetter.isOn = true;

        public void FilterNumbers() => _filterNumber.isOn = true;

        public void FilterRunes() => _filterRune.isOn = true;

        public void FilterLine() => _filterLine.isOn = true;

        private UIDecalPaletteItem GetItem(int index) => _list.transform.GetChild(index).GetComponent<UIDecalPaletteItem>();

        private void UpdatePreview()
        {
            _selectedPreview.sprite = _selected.sprite;
            _selectedPreview.gameObject.SetActive(_selected.sprite != null);
            _selectedPreview.transform.localScale = new Vector3(_selected.flipHorizontal ? -1 : 1, _selected.flipVertical ? -1 : 1, 1);
            _selectedPreview.transform.localRotation = Quaternion.Euler(0, 0, _selected.rotate ? -90 : 0);

            _selectedName.text = _selected.name;
        }
    }
}
