using System;
using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled.Editor
{
    class UIMultiSelectItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _name = null;
        [SerializeField] private Image _preview  = null;
        [SerializeField] private Button _deleteButton = null;

        private Tile _tile;

        public Tile tile {
            get => _tile;
            set {
                _tile = value;

                if (_name != null)
                    _name.text = _tile.displayName;

                _preview.sprite = tile.icon;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _deleteButton.onClick.AddListener(() => UIPuzzleEditor.DeselectTile(_tile));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnSelected()
        {
            UIPuzzleEditor.SelectTile(_tile);
        }
    }
}
