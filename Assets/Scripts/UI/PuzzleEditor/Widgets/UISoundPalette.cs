using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    class UISoundPalette : MonoBehaviour
    {
        [SerializeField] private UISoundPaletteItem _itemPrefab = null;
        [SerializeField] private UIList _list = null;
        [SerializeField] private ScrollRect _scrollRect = null;
        //[SerializeField] private Image _selectedPreview = null;
        [SerializeField] private TMPro.TMP_InputField _searchInput = null;
        [SerializeField] private Button _searchClearButton = null;
        [SerializeField] private bool allowNone = false;

        private Sound _selected;

        public Sound selected {
            get => _selected;
            set {
                if (value.guid == _selected.guid)
                    return;

                _selected = value;

                for (int i = _list.itemCount - 1; i >= 0; i--)
                {
                    var item = GetItem(i);
                    if (item.sound.guid == _selected.guid)
                    {
                        item.selected = true;
                        if (item.gameObject.activeSelf)
                            _scrollRect.ScrollTo(item.GetComponent<RectTransform>());
                        break;
                    }
                }

                //_selectedPreview.sprite = _selected.sprite;
            }
        }

        public event Action<Sound> onDoubleClickSound;

        private void Awake()
        {
            _list.onDoubleClickItem += (index) => {
                onDoubleClickSound?.Invoke(_list.GetItem(index).GetComponent<UISoundPaletteItem>().sound);
            };

            _list.onSelectionChanged += (index) => {
                _selected = _list.selectedItem?.GetComponent<UISoundPaletteItem>().sound ?? Sound.none;
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

            // Add a none sound
            Instantiate(_itemPrefab, _list.transform).GetComponent<UISoundPaletteItem>().sound = Sound.none;

            // Add all sounds to the palette
            foreach (var sound in DatabaseManager.GetSounds())
                Instantiate(_itemPrefab, _list.transform).GetComponent<UISoundPaletteItem>().sound = sound;

            _list.SelectItem(0);
        }

        private void UpdateFilter()
        {
            var checkText = _searchInput.text.Length > 0;
            var text = _searchInput.text.ToLower();

            for (int i = allowNone ? 1 : 0; i < _list.itemCount; i++)
            {
                var item = _list.GetItem(i).GetComponent<UISoundPaletteItem>();
                var sound = item.sound;
                var clipName = sound.clip != null ? sound.clip.name : "None";

                var active = true;
                active &= !checkText || (clipName.ToLower().Contains(text));
                
                _list.GetItem(i).gameObject.SetActive(active);
            }
        }

        private UISoundPaletteItem GetItem(int index) => _list.transform.GetChild(index).GetComponent<UISoundPaletteItem>();
    }
}
