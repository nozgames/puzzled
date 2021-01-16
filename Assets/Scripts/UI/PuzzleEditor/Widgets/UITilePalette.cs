using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Puzzled.UI;

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

        [SerializeField] private UIRadio _filterAll = null;
        [SerializeField] private UIRadio _filterLogic = null;
        [SerializeField] private UIRadio _filterDynamic = null;
        [SerializeField] private UIRadio _filterFloor = null;
        [SerializeField] private UIRadio _filterStatic = null;

        private Type _componentFilter;
        private Tile _selected;

        public event Action<Tile> onDoubleClickTile;

        public Tile selected {
            get => _selected;
            set {
                if (_selected != null && value != null && value.guid == _selected.guid)
                    return;

                _selected = value;

                if (_selected != null)
                    for (int i = _list.itemCount - 1; i >= 0; i--)
                    {
                        var item = GetItem(i);
                        if (item.tile.guid == _selected.guid)
                        {
                            item.selected = true;
                            if (item.gameObject.activeSelf)
                                _scrollRect.ScrollTo(item.GetComponent<RectTransform>());
                            break;
                        }
                    }

                _selectedPreview.texture = TileDatabase.GetPreview(selected);
            }
        }

        public Type componentFilter {
            get => _componentFilter;
            set {
                _componentFilter = value;
                UpdateFilter();
            }
        }

        private void Awake()
        {
            _list.onDoubleClickItem += (index) => {
                onDoubleClickTile?.Invoke(_list.GetItem(index).GetComponent<UITilePaletteItem>().tile);
            };

            _list.onSelectionChanged += (index) => {
                _selected = _list.selectedItem?.GetComponent<UITilePaletteItem>().tile;
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
            foreach (var tile in TileDatabase.GetTiles().Where(t => !t.info.isDeprecated))
                Instantiate(_itemPrefab, _list.transform).GetComponent<UITilePaletteItem>().tile = tile;

            _list.Select(0);

            UpdateFilter();

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
                active &= (_componentFilter == null) || (tile.GetComponentInChildren(_componentFilter) != null);
                    
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

        private UITilePaletteItem GetItem(int index) => _list.transform.GetChild(index).GetComponent<UITilePaletteItem>();
    }
}
