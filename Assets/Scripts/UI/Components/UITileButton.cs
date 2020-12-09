using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UITileButton : MonoBehaviour
    {
        [SerializeField] private RawImage preview = null;
        [SerializeField] private TMPro.TextMeshProUGUI nameText = null;
        [SerializeField] private Toggle _toggle = null;

        private Tile _tile;

        public Tile tile {
            get => _tile;
            set {
                _tile = value;

                if(_tile != null)
                {
                    preview.texture = TileDatabase.GetPreview(tile);
                    nameText.text = tile.info.displayName;
                }
            }
        }

        private void OnEnable()
        {
            _toggle.group = GetComponentInParent<ToggleGroup>();
        }
    }
}
