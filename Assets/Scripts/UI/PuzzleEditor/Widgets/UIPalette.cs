using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    class UIPalette : MonoBehaviour
    {
        [SerializeField] private UITileItem _tilePrefab = null;
        [SerializeField] private UIDecalPaletteItem _decalPrefab = null;
        [SerializeField] private UIList _tiles = null;
        [SerializeField] private UIList _decals = null;
        [SerializeField] private ScrollRect _scrollRect = null;

        private Decal _decalNone;
        private Type _itemType;

        /// <summary>
        /// Set the current item type being displayed
        /// </summary>
        public Type itemType {
            get => _itemType;
            set {
                _itemType = value;

                if (gameObject.activeSelf)
                    UpdateItemType();               
            }
        }

        /// <summary>
        /// Get the selected item of the given type
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <returns>Selected item or null</returns>
        public T GetSelected<T>() => (T)(GetList(typeof(T)).selectedItem?.GetComponent<UIPaletteItem>().value);

        private UIList GetList(Type type)
        {
            if (_itemType == typeof(Tile))
                return _tiles;
            else if (_itemType == typeof(Decal))
                return _decals;
            else
                throw new NotImplementedException();
        }

        private void OnEnable()
        {
            if (_tiles.itemCount == 0)
                Initialize();

            UpdateItemType();
        }       

        private void UpdateItemType()
        {
            _tiles.gameObject.SetActive(false);
            _decals.gameObject.SetActive(false);

            if (null == _itemType)
                return;

            var list = GetList(_itemType);
            list.gameObject.SetActive(true);
            _scrollRect.ScrollTo(list.selectedItem.GetComponent<RectTransform>());
        }

        private void Initialize()
        {
            // Add a none tile
            Instantiate(_tilePrefab, _tiles.transform).GetComponent<UITileItem>().tile = null;

            // Add all tiles
            foreach (var tile in TileDatabase.GetTiles())
                Instantiate(_tilePrefab, _tiles.transform).GetComponent<UITileItem>().tile = tile;

            // Add a none decal
            _decalNone = new Decal(Guid.Empty, null);
            Instantiate(_decalPrefab, _decals.transform).GetComponent<UIDecalPaletteItem>().decal = _decalNone;
            
            // Add all decals to the palette
            foreach (var decal in DecalDatabase.GetDecals())
                Instantiate(_decalPrefab, _decals.transform).GetComponent<UIDecalPaletteItem>().decal = decal;

            _tiles.Select(0);
            _decals.Select(0);
        }

        /// <summary>
        /// Filter the list by the given item type
        /// </summary>
        public int Filter (Type itemType)
        {
#if false
            var first = -1;
            for (int i = 0; i < _list.itemCount; i++)
            {
                var child = _list.GetItem(i);
                child.gameObject.SetActive(child.GetComponent(itemType) != null);
                if (child.gameObject.activeSelf && first == -1)
                    first = i;
            }

            return first;
#else
            return 0;
#endif
        }
    }
}
