using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    public class UIBackgroundPalette : MonoBehaviour
    {
        [SerializeField] private UIBackgroundPaletteItem _itemPrefab = null;
        [SerializeField] private UIList _list = null;
        [SerializeField] private Image _selectedPreview = null;
        [SerializeField] private TMPro.TextMeshProUGUI _selectedName = null;
        [SerializeField] private TMPro.TMP_InputField _searchInput = null;
        [SerializeField] private Button _searchClearButton = null;
        [SerializeField] private ScrollRect _scrollRect = null;

        private Background _selected;

        private UIBackgroundPaletteItem GetItem(int index) => _list.transform.GetChild(index).GetComponent<UIBackgroundPaletteItem>();

        public Background selected {
            get => _selected;
            set {
                if (_selected != null && value != null && value.guid == _selected.guid)
                    return;

                _selected = value;

                if (_selected != null)
                    for (int i = _list.itemCount - 1; i >= 0; i--)
                    {
                        var item = GetItem(i);
                        if (item.background.guid == _selected.guid)
                        {
                            item.selected = true;
                            if (item.gameObject.activeSelf)
                                _scrollRect.ScrollTo(item.GetComponent<RectTransform>());
                            break;
                        }
                    }

                UpdatePreview();
            }
        }

        public event Action<Background> onDoubleClickBackground;

        private void Awake()
        {
            _list.onDoubleClickItem += (index) => {
                onDoubleClickBackground?.Invoke(_list.GetItem(index).GetComponent<UIBackgroundPaletteItem>().background);
            };

            _list.onSelectionChanged += (index) => {
                selected = _list.selectedItem?.GetComponent<UIBackgroundPaletteItem>().background;
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
            Instantiate(_itemPrefab, _list.transform).GetComponent<UIBackgroundPaletteItem>().background = null;

            // Add all decals to the palette
            foreach (var background in DatabaseManager.GetBackgrounds())
                Instantiate(_itemPrefab, _list.transform).GetComponent<UIBackgroundPaletteItem>().background = background;

            _list.Select(0);
        }

        private void UpdateFilter()
        {
            var checkText = _searchInput.text.Length > 0;
            var text = _searchInput.text.ToLower();

            for (int i = 1; i < _list.itemCount; i++)
            {
                var item = _list.GetItem(i).GetComponent<UIBackgroundPaletteItem>();
                var background = item.background;
                var backgroundName = background != null ? background.name : "None";

                var active = true;
                active &= !checkText || (backgroundName.ToLower().Contains(text));

                _list.GetItem(i).gameObject.SetActive(active);
            }
        }

        private void UpdatePreview()
        {
            _selectedPreview.gameObject.SetActive(_selected != null);
            if (_selected != null)
                _selectedPreview.color = _selected.color;

            _selectedName.text = _selected == null ? "None" : _selected.name;
        }
    }
}
