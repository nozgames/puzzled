using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UITileSelectorTile : MonoBehaviour
    {
        [SerializeField] private RawImage _preview = null;
        [SerializeField] private Button _button = null;
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;

        private Tile _tile;

        public Button button => _button;

        public Tile tile {
            get => _tile;
            set {
                _tile = value;
                _nameText.text = _tile.name;
                _preview.texture = TileDatabase.GetPreview(_tile);
            }
        }
    }
}
