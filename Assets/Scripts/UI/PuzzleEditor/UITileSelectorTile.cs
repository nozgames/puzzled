using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UITileSelectorTile : MonoBehaviour
    {
        [SerializeField] private Image _preview = null;
        [SerializeField] private Button _button = null;
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;

        private Tile _tile;

        public Button button => _button;

        public Tile tile {
            get => _tile;
            set {
                _tile = value;
                _nameText.text = _tile.name;
                var texture = TileDatabase.GetPreview(_tile);
                if (texture != null)
                    _preview.sprite = Sprite.Create(TileDatabase.GetPreview(_tile), new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                else
                    _preview.sprite = null;
            }
        }
    }
}
