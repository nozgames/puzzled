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
        [SerializeField] private UIDecalPreview _selectedPreview = null;
        [SerializeField] private UIDecalEditor _decalProperties = null;
        [SerializeField] private TMPro.TextMeshProUGUI _selectedName = null;
        [SerializeField] private TMPro.TMP_InputField _searchInput = null;
        [SerializeField] private Button _searchClearButton = null;
        [SerializeField] private Button _importButton = null;
        [SerializeField] private Button _defaultsButton = null;
        [SerializeField] private bool allowNone = false;

        [SerializeField] private UIRadio _filterAll = null;
        [SerializeField] private UIRadio _filterLetter = null;
        [SerializeField] private UIRadio _filterRune = null;
        [SerializeField] private UIRadio _filterNumber = null;
        [SerializeField] private UIRadio _filterLine = null;

        private Decal _selected = Decal.none;
        
        public Decal selected {
            get => _selected;
            set => SetSelected(value, true);
        }

        public event Action<Decal> onDoubleClickDecal;

        private class TargetProperty : IPropertyEditorTarget
        {
            public UIDecalPalette _palette;

            public string id => "id";

            public string name => _palette._selected.name;

            public string placeholder => null;

            public Vector2Int range => Vector2Int.zero;

            public object GetValue() => _palette._selected;

            public T GetValue<T>() => (T)GetValue();

            public void SetValue(object value, bool commit = true) 
            {
                if(commit)
                    _palette.selected = (Decal) value;
                else
                {
                    _palette._selected = (Decal)value;
                    _palette._selectedPreview.decal = _palette._selected;
                }
            }
        }

        private void Awake()
        {
            _list.onDoubleClickItem += (index) => {
                onDoubleClickDecal?.Invoke(_list.GetItem(index).GetComponent<UIDecalPaletteItem>().decal);
            };

            _list.onSelectionChanged += (index) => {
                _selected.SetTexture(_list.selectedItem?.GetComponent<UIDecalPaletteItem>().decal ?? Decal.none);
                UpdatePreview();
            };

            _searchClearButton.onClick.AddListener(() => {
                _searchInput.text = "";
                EventSystem.current.SetSelectedGameObject(_searchInput.gameObject);
            });
            _searchClearButton.gameObject.SetActive(false);

            _importButton.onClick.AddListener(() => {
                UIPuzzleEditor.Import();
            });

            _defaultsButton.onClick.AddListener(() => {
                var decal = Decal.none;
                decal.SetTexture(_selected);
                SetSelected(decal, false);
            });

            _searchInput.onValueChanged.AddListener((text) => {
                _searchClearButton.gameObject.SetActive(!string.IsNullOrEmpty(text));
                UpdateFilter();
            });

            // Add a none decal
            if(allowNone)
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = Decal.none;

            // Add all decals to the palette
            foreach (var decal in DatabaseManager.GetDecals())
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = decal;

            _filterAll.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterNumber.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterRune.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterLetter.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterLine.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });

            UpdatePreview();
        }

        public void RemoveImportedDecals ()
        {
            for(var childIndex = _list.transform.childCount - 1; childIndex >=0; childIndex--)
            {
                var item = GetItem(childIndex);
                if (item.decal.isImported)
                    Destroy(item.gameObject);
            }
        }

        public void AddDecal (Decal decal)
        {
            Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = decal;
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
                var spriteName = decal.name;

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
            _selectedPreview.decal = _selected;
            _selectedName.text = _selected.name;
            _decalProperties.target = new TargetProperty { _palette = this };
        }
    }
}
