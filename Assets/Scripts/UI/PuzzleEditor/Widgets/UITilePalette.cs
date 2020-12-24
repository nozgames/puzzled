using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    class UITilePalette : MonoBehaviour
    {
        [SerializeField] private UITilePaletteItem _itemPrefab = null;
        [SerializeField] private UIList _list = null;
        [SerializeField] private ScrollRect _scrollRect = null;
        [SerializeField] private RawImage _selectedPreview = null;
        [SerializeField] private bool allowNone = false;
        [SerializeField] private TMPro.TMP_InputField _searchInput = null;
        [SerializeField] private Button _searchClearButton = null;

        [SerializeField] private Toggle _filterAll = null;
        [SerializeField] private Toggle _filterLogic = null;
        [SerializeField] private Toggle _filterDynamic = null;
        [SerializeField] private Toggle _filterFloor = null;
        [SerializeField] private Toggle _filterStatic = null;

        public Tile selected { get; private set; }

        private void Awake()
        {
            _list.onSelectionChanged += (index) => {
                selected = _list.selectedItem?.GetComponent<UITilePaletteItem>().tile;
                _selectedPreview.texture = TileDatabase.GetPreview(selected);
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

            // Add a none item
            if (allowNone)
                Instantiate(_itemPrefab, _list.transform).GetComponent<UITilePaletteItem>().tile = null;

            // Add all tiles to the palette
            foreach (var tile in TileDatabase.GetTiles())
                Instantiate(_itemPrefab, _list.transform).GetComponent<UITilePaletteItem>().tile = tile;

            _list.Select(0);

            _filterAll.onValueChanged.AddListener(OnLayerValueChanged);
            _filterLogic.onValueChanged.AddListener(OnLayerValueChanged);
            _filterDynamic.onValueChanged.AddListener(OnLayerValueChanged);
            _filterStatic.onValueChanged.AddListener(OnLayerValueChanged);
            _filterFloor.onValueChanged.AddListener(OnLayerValueChanged);
        }

        private void OnLayerValueChanged(bool v)
        {
            if (v)
                UpdateFilter();
        }

        private void UpdateFilter()
        {
            var checkText = _searchInput.text.Length > 0;
            var text = _searchInput.text.ToLower();
            var checkLayer = !_filterAll.isOn;
            var layer = TileLayer.Floor;
            if (_filterDynamic.isOn)
                layer = TileLayer.Dynamic;
            else if (_filterLogic.isOn)
                layer = TileLayer.Logic;
            else if (_filterStatic.isOn)
                layer = TileLayer.Static;

            for (int i = allowNone ? 1 : 0; i < _list.itemCount; i++)
            {
                var item = _list.GetItem(i).GetComponent<UITilePaletteItem>();
                var tile = item.tile;

                var active = true;
                active &= !checkText || tile.name.ToLower().Contains(text);
                active &= !checkLayer || tile.info.layer == layer;
                    
                _list.GetItem(i).gameObject.SetActive(active);
            }
        }

        public void FilterAll() => _filterAll.isOn = true;

        public void FilterLayer(TileLayer layer)
        {
            switch (layer)
            {
                case TileLayer.Logic:
                    _filterLogic.isOn = true;
                    break;
                case TileLayer.Dynamic:
                    _filterDynamic.isOn = true;
                    break;
                case TileLayer.Floor:
                    _filterFloor.isOn = true;
                    break;
                case TileLayer.Static:
                    _filterStatic.isOn = true;
                    break;
            }
        }
    }
}
