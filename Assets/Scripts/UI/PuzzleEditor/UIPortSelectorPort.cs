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

                if(!string.IsNullOrEmpty(_to.customIcon))
                {
                    var propertyInfo = _to.component.GetType().GetProperty(_to.customIcon);
                    if(propertyInfo.PropertyType == typeof(Sprite))
                    {
                        _icon.sprite = propertyInfo.GetValue(to.component) as Sprite;
                        return;
                    }
                }

                _icon.sprite = DatabaseManager.GetPortIcon(_to);
            }
        }
    }
}
