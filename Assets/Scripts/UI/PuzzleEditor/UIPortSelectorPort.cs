using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIPortSelectorPort : MonoBehaviour
    {
        [SerializeField] private Image _icon = null;
        [SerializeField] private Button _button = null;
        [SerializeField] private TMPro.TextMeshProUGUI _nameText = null;

        private Port _to;

        public Button button => _button;

        public Port from { get; set; }

        public Port to 
        {
            get => _to;
            set {
                _to = value;
                _nameText.text = _to.displayName;
                _icon.sprite = TileDatabase.GetPortIcon(_to);
            }
        }
    }
}
