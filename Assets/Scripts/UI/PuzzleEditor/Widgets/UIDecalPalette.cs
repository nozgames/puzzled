using System;
using System.Text.RegularExpressions;
using UnityEngine;
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

        [SerializeField] private Toggle _filterAll = null;
        [SerializeField] private Toggle _filterLetter = null;
        [SerializeField] private Toggle _filterRune = null;
        [SerializeField] private Toggle _filterNumber = null;
        [SerializeField] private Toggle _filterLine = null;

        public Decal selected { get; private set; }

        public event Action<Decal> onDoubleClickDecal;

        private void Awake()
        {
            _list.onDoubleClickItem += (index) => {
                onDoubleClickDecal?.Invoke(_list.GetItem(index).GetComponent<UIDecalPaletteItem>().decal);
            };

            _list.onSelectionChanged += (index) => {
                selected = _list.selectedItem?.GetComponent<UIDecalPaletteItem>().decal;
            };

            // Add a none decal
            Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = null;

            // Add all decals to the palette
            foreach (var decal in DecalDatabase.GetDecals())
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIDecalPaletteItem>().decal = decal;

            _list.Select(0);

            _filterAll.onValueChanged.AddListener((v) => { if (v) Filter(null, null); });
            _filterNumber.onValueChanged.AddListener((v) => { if (v) Filter(null, NumberRegex); });
            _filterRune.onValueChanged.AddListener((v) => { if (v) Filter(null, RuneRegex); });
            _filterLetter.onValueChanged.AddListener((v) => { if (v) Filter(null, LetterRegex); });
            _filterLine.onValueChanged.AddListener((v) => { if (v) Filter(null, LineRegex); });
        }

        private void Filter(Toggle toggle, Regex regex)
        {
            if (null != toggle)
                toggle.isOn = true;

            if(null == regex)
            {
                for (int i = 1; i < _list.itemCount; i++)
                    _list.GetItem(i).GetComponent<UIDecalPaletteItem>().gameObject.SetActive(true);
            }
            else
            {
                for (int i = 1; i < _list.itemCount; i++)
                {
                    var item = _list.GetItem(i).GetComponent<UIDecalPaletteItem>();
                    item.gameObject.SetActive(regex.Match(item.decal.sprite.name).Success);
                }
            }

            if (null == _list.selectedItem || !_list.selectedItem.gameObject.activeSelf)
                _list.Select(0);

            _scrollRect.ScrollTo(_list.selectedItem.GetComponent<RectTransform>());
        }

        public void FilterAll() => Filter(_filterAll, null);

        public void FilterLetters() => Filter(_filterLetter, LetterRegex);

        public void FilterNumbers() => Filter(_filterNumber, NumberRegex);

        public void FilterRunes() => Filter(_filterRune, RuneRegex);

        public void FilterLine() => Filter(_filterLine, LineRegex);
    }
}
