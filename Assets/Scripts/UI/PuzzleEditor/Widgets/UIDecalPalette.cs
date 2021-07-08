using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Puzzled.UI;
using System.Collections.Generic;

namespace Puzzled.Editor
{
    class UIDecalPalette : MonoBehaviour
    {
        private readonly Regex LetterRegex = new Regex("(Letter).*", RegexOptions.IgnoreCase);
        private readonly Regex NumberRegex = new Regex("(Number).*", RegexOptions.IgnoreCase);
        private readonly Regex RuneRegex = new Regex("(Rune).*", RegexOptions.IgnoreCase);
        private readonly Regex ShapeRegex = new Regex("(SolidLine|DashedLine|Shape).*", RegexOptions.IgnoreCase);
        private readonly Regex SymbolRegex = new Regex("(Symbol).*", RegexOptions.IgnoreCase);

        [SerializeField] private Texture2D _noneTexture = null;
        [SerializeField] private Color _noneColor = Color.white;
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
        [SerializeField] private UIRadio _filterShape = null;
        [SerializeField] private UIRadio _filterSymbol = null;
        [SerializeField] private UIRadio _filterImported = null;

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
                UIPuzzleEditor.Import((decal) => {
                    if (decal != Decal.none)
                    {
                        _filterImported.isOn = true;
                        SetSelected(decal, true);
                    }
                });
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

            _filterAll.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterNumber.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterRune.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterLetter.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterShape.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterSymbol.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });
            _filterImported.onValueChanged.AddListener((v) => { if (v) UpdateFilter(); });

            UpdatePreview();
        }

        public void LoadDecals (World world)
        {
            // None
            if (allowNone)
            {
                var none = new Decal(Guid.Empty, _noneTexture);
                none.isAutoColor = false;
                none.color = _noneColor;
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = none;
            }

            // Add all built-in decals 
            foreach (var decal in DatabaseManager.GetDecals())
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = decal;

            // Add all the world decals
            foreach (var decal in world.decals)
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = decal;

        }

        public void UnloadDecals ()
        {
            _list.transform.DetachAndDestroyChildren();
        }

        public void AddDecal (Decal decal)
        {
            Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = decal;
        }

        private void OnEnable()
        {
            if (_list.selected == -1)
                _list.SelectItem(0);

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
            else if (_filterShape.isOn)
                regex = ShapeRegex;
            else if (_filterSymbol.isOn)
                regex = SymbolRegex;

            for (int i = allowNone ? 1 : 0; i < _list.itemCount; i++)
            {
                var item = _list.GetItem(i).GetComponent<UIDecalPaletteItem>();
                var decal = item.decal;
                var spriteName = decal.name;

                var active = true;
                active &= !checkText || (spriteName.ToLower().Contains(text));
                active &= (regex == null) || (regex.Match(spriteName).Success);

                if (decal.isImported)
                    active &= (_filterImported.isOn || _filterAll.isOn);
                else
                    active &= !_filterImported.isOn;

                _list.GetItem(i).gameObject.SetActive(active);
            }
        }

        public void FilterAll() => _filterAll.isOn = true;

        public void FilterLetters() => _filterLetter.isOn = true;

        public void FilterNumbers() => _filterNumber.isOn = true;

        public void FilterRunes() => _filterRune.isOn = true;

        public void FilterShape() => _filterShape.isOn = true;

        public void FilterSymbol() => _filterSymbol.isOn = true;

        private UIDecalPaletteItem GetItem(int index) => _list.transform.GetChild(index).GetComponent<UIDecalPaletteItem>();

        private void UpdatePreview()
        {
            _selectedPreview.decal = _selected;
            if(_selectedName != null)
                _selectedName.text = _selected.name;
            _decalProperties.target = new TargetProperty { _palette = this };
            _defaultsButton.transform.parent.gameObject.SetActive(_selected != Decal.none);
        }
    }
}
