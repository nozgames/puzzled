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
                var texture = DatabaseManager.GetPreview(_tile);
                if (texture != null)
                    _preview.sprite = DatabaseManager.GetPreview(_tile);
                else
                    _preview.sprite = null;
            }
        }
    }
}
