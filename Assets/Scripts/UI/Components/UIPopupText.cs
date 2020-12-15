using UnityEngine;

namespace Puzzled
{
    public class UIPopupText : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;

        public string text {
            get => _text.text;
            set => _text.text = value;
        }
    }
}
