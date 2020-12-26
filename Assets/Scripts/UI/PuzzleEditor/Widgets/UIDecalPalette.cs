using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        [SerializeField] private TMPro.TMP_InputField _searchInput = null;
        [SerializeField] private Button _searchClearButton = null;
        [SerializeField] private bool allowNone = false;

        [SerializeField] private Toggle _filterAll = null;
        [SerializeField] private Toggle _filterLetter = null;
        [SerializeField] private Toggle _filterRune = null;
        [SerializeField] private Toggle _filterNumber = null;
        [SerializeField] private Toggle _filterLine = null;

        private Decal _selected;

        public Decal selected {
            get => _selected;
            set {
                if (_selected != null && value != null && value.guid == _selected.guid)
                    return;

                _selected = value;

                if (_selected != null)
                    for (int i = _list.itemCount - 1; i >= 0; i--)
                    {
                        var item = GetItem(i);
                        if (item.decal.guid == _selected.guid)
                        {
                            item.selected = true;
                            if (item.gameObject.activeSelf)
                                _scrollRect.ScrollTo(item.GetComponent<RectTransform>());
                            break;
                        }
                    }

                _selectedPreview.sprite = _selected.sprite;
            }
        }

        public event Action<Decal> onDoubleClickDecal;

        private void Awake()
        {
            _list.onDoubleClickItem += (index) => {
                onDoubleClickDecal?.Invoke(_list.GetItem(index).GetComponent<UIDecalPaletteItem>().decal);
            };

            _list.onSelectionChanged += (index) => {
                selected = _list.selectedItem?.GetComponent<UIDecalPaletteItem>().decal ?? Decal.none;
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
            Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = Decal.none;

            // Add all decals to the palette
            foreach (var decal in DecalDatabase.GetDecals())
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = decal;

            _list.Select(0);

            _filterAll.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterNumber.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterRune.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterLetter.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterLine.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
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
    }
}
